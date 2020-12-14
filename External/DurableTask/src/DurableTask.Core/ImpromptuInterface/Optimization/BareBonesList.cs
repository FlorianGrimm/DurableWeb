// 
//  Copyright 2011 Ekon Benefits
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace ImpromptuInterface.Optimization
{
    internal class BareBonesList<T>: ICollection<T>
    {
        private T[] _list;
        private int _addIndex;
   
        private int _length;


        /// <summary>
        /// Initializes a new instance of the <see cref="BareBonesList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="length">The max length that the list cannot grow beyound</param>
        public BareBonesList(int length)
        {
            this._list = new T[length];
            this._length = length;
        }

        public void Add(T item)
        {
            this._list[this._addIndex++] = item;
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._list,arrayIndex,array,0, this._length);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return this._length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the enumerator. with bare bones this is good only once
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new BareBonesEnumerator(this._list, this._addIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        internal class BareBonesEnumerator : IEnumerator<T>

        {
            private T[] _list;
            private int _enumerateInex = -1;
            private int _length;

            public BareBonesEnumerator(T[] list, int length)
            {
                this._list = list;
                this._length = length;
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                this._enumerateInex++;
                return this._enumerateInex < this._length;
            }

            public void Reset()
            {
                this._enumerateInex = 0;
            }

            public T Current
            {
                get { return this._list[this._enumerateInex]; }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
        }
    
    }

 
}