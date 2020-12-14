using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dynamitey.Internal.Optimization;
using Microsoft.CSharp.RuntimeBinder;
using System.Reflection;
using Dynamitey.Internal.Compat;

namespace Dynamitey
{
    /// <summary>
    /// Cacheable representation of an invocation without the target or arguments  also by default only does public methods to make it easier to cache.
    ///  /// </summary>

    public class CacheableInvocation:Invocation
    {
        /// <summary>
        /// Creates the cacheable convert call.
        /// </summary>
        /// <param name="convertType">Type of the convert.</param>
        /// <param name="convertExplicit">if set to <c>true</c> [convert explicit].</param>
        /// <returns></returns>
        public static CacheableInvocation CreateConvert(Type convertType, bool convertExplicit=false)
        {
            return new CacheableInvocation(InvocationKind.Convert, convertType: convertType, convertExplicit: convertExplicit);
        }

        /// <summary>
        /// Creates the cacheable method or indexer or property call.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="name">The name.</param>
        /// <param name="callInfo">The callInfo.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static CacheableInvocation CreateCall(InvocationKind kind, String_OR_InvokeMemberName name = null, CallInfo callInfo = null,object context = null)
        {
            var tArgCount = callInfo?.ArgumentCount ?? 0;
            var tArgNames = callInfo?.ArgumentNames.ToArray();

            return new CacheableInvocation(kind, name, tArgCount, tArgNames, context);
        }

        private readonly int _argCount;
        private readonly string[] _argNames;
        private readonly bool _staticContext;
        private readonly Type _context;

        //[NonSerialized]
        private CallSite _callSite;
        //[NonSerialized]
        private CallSite _callSite2;
        //[NonSerialized]
        private CallSite _callSite3;
        //[NonSerialized]
        private CallSite _callSite4;

        private readonly bool _convertExplicit;
        private readonly Type _convertType;

     

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableInvocation"/> class.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="name">The name.</param>
        /// <param name="argCount">The arg count.</param>
        /// <param name="argNames">The arg names.</param>
        /// <param name="context">The context.</param>
        /// <param name="convertType">Type of the convert.</param>
        /// <param name="convertExplicit">if set to <c>true</c> [convert explict].</param>
        /// <param name="storedArgs">The stored args.</param>
        public CacheableInvocation(InvocationKind kind,
                                   String_OR_InvokeMemberName name=null,
                                   int argCount =0,
                                   string[] argNames =null,
                                   object context = null,
                                   Type convertType = null,
                                   bool convertExplicit = false, 
                                   object[] storedArgs = null)
            : base(kind, name, storedArgs)
        {

            this._convertType = convertType;
            this._convertExplicit = convertExplicit;

            this._argNames = argNames ?? new string[] {};

            if (storedArgs != null)
            {
                this._argCount = storedArgs.Length;
                this.Args = Util.GetArgsAndNames(storedArgs, out var tArgNames);
                if (this._argNames.Length < tArgNames.Length)
                {
                    this._argNames = tArgNames;
                }
            }

            switch (kind) //Set required argcount values
            {
                case InvocationKind.GetIndex:
                    if (argCount < 1)
                    {
                        throw new ArgumentException("Arg Count must be at least 1 for a GetIndex", nameof(argCount));
                    }
                    this._argCount = argCount;
                    break;
                case InvocationKind.SetIndex:
                    if (argCount < 2)
                    {
                        throw new ArgumentException("Arg Count Must be at least 2 for a SetIndex", nameof(argCount));
                    }
                    this._argCount = argCount;
                    break;
                case InvocationKind.Convert:
                    this._argCount = 0;
                    if(convertType==null)
                        throw new ArgumentNullException(nameof(convertType)," Convert Requires Convert Type ");
                    break;
                case InvocationKind.SubtractAssign:
                case InvocationKind.AddAssign:
                case InvocationKind.Set:
                    this._argCount = 1;
                    break;
                case InvocationKind.Get:
                case InvocationKind.IsEvent:
                    this._argCount = 0;
                    break;
                default:
                    this._argCount = Math.Max(argCount, this._argNames.Length);
                    break;
            }

            if (this._argCount > 0)//setup argName array
            {
                var tBlank = new string[this._argCount];
                if (this._argNames.Length != 0)
                    Array.Copy(this._argNames, 0, tBlank, tBlank.Length - this._argNames.Length, tBlank.Length);
                else
                    tBlank = null;
                this._argNames = tBlank;
            }


            if (context != null)
            {
                var dummy = context.GetTargetContext(out this._context, out this._staticContext); //lgtm [cs/useless-assignment-to-local]
            }
            else
            {
                this._context = typeof (object);
            }


        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(CacheableInvocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) 
                && other._argCount == this._argCount 
                && Equals(other._argNames, this._argNames) 
                && other._staticContext.Equals(this._staticContext)
                && Equals(other._context, this._context) 
                && other._convertExplicit.Equals(this._convertExplicit)
                && Equals(other._convertType, this._convertType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return this.Equals(obj as CacheableInvocation);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result*397) ^ this._argCount;
                result = (result*397) ^ (this._argNames != null ? this._argNames.GetHashCode() : 0);
                result = (result*397) ^ this._staticContext.GetHashCode();
                result = (result*397) ^ (this._context != null ? this._context.GetHashCode() : 0);
                result = (result*397) ^ this._convertExplicit.GetHashCode();
                result = (result*397) ^ (this._convertType != null ? this._convertType.GetHashCode() : 0);
                return result;
            }
        }


        /// <summary>
        /// Invokes the invocation on specified target with specific args.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">CacheableInvocation can't change conversion type on invoke.;args</exception>
        /// <exception cref="System.InvalidOperationException">Unknown Invocation Kind: </exception>
        public override object Invoke(object target, params object[] args)
        {
            if (target is InvokeContext tIContext)
            {
                target = tIContext.Target;
            }

            if (args == null)
            {
                args = new object[]{null};
            }
           

            if (args.Length != this._argCount)
            {
                switch (this.Kind)
                {
                    case InvocationKind.Convert:
                        if (args.Length > 0)
                        {
                            if (!Equals(args[0], this._convertType))
                                throw new ArgumentException("CacheableInvocation can't change conversion type on invoke.", nameof(args));
                        }
                        if (args.Length > 1)
                        {
                            if(!Equals(args[1], this._convertExplicit))
                                throw new ArgumentException("CacheableInvocation can't change explicit/implicit conversion on invoke.", nameof(args));
                        }

                        if(args.Length > 2)
                            goto default;
                        break;
                    default:
                        throw new ArgumentException("args",
                            $"Incorrect number of Arguments for CachedInvocation, Expected:{this._argCount}");
                }
            }

            switch (this.Kind)
            {
                case InvocationKind.Constructor:
                    var tTarget = (Type) target;
                    return InvokeHelper.InvokeConstructorCallSite(tTarget, tTarget.GetTypeInfo().IsValueType, args, this._argNames,
                                                                  ref this._callSite);
                case InvocationKind.Convert:
                    return InvokeHelper.InvokeConvertCallSite(target, this._convertExplicit, this._convertType, this._context,
                                                              ref this._callSite);
                case InvocationKind.Get:
                    return InvokeHelper.InvokeGetCallSite(target, this.Name.Name, this._context, this._staticContext, ref this._callSite);
                case InvocationKind.Set:
                    InvokeHelper.InvokeSetCallSite(target, this.Name.Name, args[0], this._context, this._staticContext, ref this._callSite);
                    return null;
                case InvocationKind.GetIndex:
                    return InvokeHelper.InvokeGetIndexCallSite(target, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                case InvocationKind.SetIndex:
                    Dynamic.InvokeSetIndex(target, args);
                    return null;
                case InvocationKind.InvokeMember:
                    return InvokeHelper.InvokeMemberCallSite(target, (InvokeMemberName)this.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                case InvocationKind.InvokeMemberAction:
                    InvokeHelper.InvokeMemberActionCallSite(target, (InvokeMemberName)this.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                    return null;
                case InvocationKind.InvokeMemberUnknown:
                    {
                       
                            try
                            {
                                var tObj = InvokeHelper.InvokeMemberCallSite(target, (InvokeMemberName)this.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                                return tObj;
                            }
                            catch (RuntimeBinderException)
                            {
                                InvokeHelper.InvokeMemberActionCallSite(target, (InvokeMemberName)this.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite2);
                            return null;

                            }
                          
                    }
                case InvocationKind.Invoke:
                    return InvokeHelper.InvokeDirectCallSite(target, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                case InvocationKind.InvokeAction:
                    InvokeHelper.InvokeDirectActionCallSite(target, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                    return null;
                case InvocationKind.InvokeUnknown:
                    {

                        try
                        {
                            var tObj = InvokeHelper.InvokeDirectCallSite(target, args, this._argNames, this._context, this._staticContext, ref this._callSite);
                            return tObj;
                        }
                        catch (RuntimeBinderException)
                        {
                            InvokeHelper.InvokeDirectActionCallSite(target, args, this._argNames, this._context, this._staticContext, ref this._callSite2);
                            return null;

                        }
                    }
                case InvocationKind.AddAssign:
                    InvokeHelper.InvokeAddAssignCallSite(target, this.Name.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite, ref this._callSite2, ref this._callSite3, ref this._callSite4);
                    return null;
                case InvocationKind.SubtractAssign:
                    InvokeHelper.InvokeSubtractAssignCallSite(target, this.Name.Name, args, this._argNames, this._context, this._staticContext, ref this._callSite, ref this._callSite2, ref this._callSite3, ref this._callSite4);
                    return null;
                case InvocationKind.IsEvent:
                    return InvokeHelper.InvokeIsEventCallSite(target, this.Name.Name, this._context, ref this._callSite);
                default:
                    throw new InvalidOperationException("Unknown Invocation Kind: " + this.Kind);
            }
        }

       
    }
}