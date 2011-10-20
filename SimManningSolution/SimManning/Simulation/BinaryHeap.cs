using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimManning.Simulation
{
	/// <summary>
	/// An implicit binary heap used for priority queues.
	/// </summary>
	/// <remarks>
	/// Inspired from
	/// http://www.mischel.com/pubs/priqueue.zip ( http://www.devsource.com/c/a/Languages/A-Priority-Queue-Implementation-in-C/1/ )
	/// http://gpwiki.org/index.php/C_sharp:BinaryHeapOfT
	/// </remarks>
	/// <typeparam name="T"><![CDATA[IComparable<T> type of item in the heap]]>.</typeparam>
	public sealed class BinaryHeap<T> : ICollection, ICollection<T> where T : IComparable<T>
	{
		const int MinimalCapacity = 16;

		T[] data;
		int count;
		int capacity;
		bool sorted;

		/// <summary>
		/// Creates a new binary heap.
		/// </summary>
		public BinaryHeap(int capacity = MinimalCapacity)
		{
			this.capacity = capacity;
			this.data = new T[capacity];
		}

		#region Helpers for implicit binary heap logics
		/// <summary>
		/// Helper function that calculates the parent of a node
		/// </summary>
		static int Parent(int index)
		{
			return (index - 1) >> 1;	//N.B.: "i >> 1" is floor(i / 2.0) while "i / 2" is trunc(i / 2.0). So "-1 >> 1 == -1", but "-1 / 2 == 0".
		}

		/// <summary>
		/// Helper function that calculates the first child of a node
		/// </summary>
		static int Child1(int index)
		{
			return (index * 2) + 1;	//For discussion on "i * 2" vs. "i << 1" see http://stackoverflow.com/questions/1945719/is-there-a-way-to-see-the-native-code-produced-by-thejitter-for-given-c-cil
		}

		/// <summary>
		/// Helper function that calculates the second child of a node
		/// </summary>
		static int Child2(int index)
		{
			return (index * 2) + 2;
		}

		/// <summary>
		/// Helper function that performs up-heap bubbling
		/// </summary>
		void BubbleUp(int p)
		{
			var init = p;
			var item = this.data[p];
			var par = Parent(p);
			while ((par >= 0) && (item.CompareTo(this.data[par]) < 0))
			{
				this.data[p] = this.data[par];	//Swap nodes
				p = par;
				par = Parent(p);
			}
			this.data[p] = item;
			this.sorted &= (init == p);
		}

		/// <summary>
		/// Helper function that performs down-heap bubbling
		/// </summary>
		int BubbleDown(int p = 0)
		{
			var init = p;
			int n;
			var item = this.data[p];
			while (true)
			{
				var ch1 = Child1(p);
				if (ch1 >= this.count) break;
				var ch2 = Child2(p);
				if (ch2 >= this.count) n = ch1;
				else n = this.data[ch1].CompareTo(this.data[ch2]) < 0 ? ch1 : ch2;
				if (item.CompareTo(this.data[n]) <= 0) break;
				this.data[p] = this.data[n];	//Swap nodes
				p = n;
			}
			this.data[p] = item;
			this.sorted &= (init == p);
			#if (DEBUG)
			if ((this.count > MinimalCapacity) && (this.count < this.capacity / 4))	//Reduction of capacity if less than 25% is used
				Capacity = this.count;	//TODO: Try to see if this is ever used in practice
			#endif
			return p;
		}
		#endregion

		#region Partial mimic of Queue<T>
		/// <summary>
		/// Gets the first value in the heap without removing it.
		/// </summary>
		/// <returns>The lowest value of type TValue.</returns>
		public T Peek()
		{
			return this.data[0];
		}
		#endregion

		#region List<T> Partial mimic and IList<T> partial interface
		public T this[int index]
		{
			get
			{
				return this.data[index];
			}
			set
			{
				this.data[index] = value;
			}
		}

		/// <summary>
		/// Gets or sets the capacity of the heap.
		/// </summary>
		public int Capacity
		{
			get { return this.capacity; }
			set
			{
				int previousCapacity = this.capacity;
				this.capacity = Math.Max(MinimalCapacity, Math.Max(value, this.count));
				if (this.capacity != previousCapacity)
					Array.Resize(ref this.data, this.capacity);
			}
		}

		/// <summary>
		/// Adds the elements of the specified collection.
		/// </summary>
		/// <param name="collection">The collection whose elements should be added.
		/// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type</param>
		public void AddRange(ICollection<T> collection)
		{
			var newCapacity = this.count + collection.Count;
			if (newCapacity > this.capacity)
				Capacity = newCapacity;
			foreach (var item in collection)
				this.data[this.count++] = item;
			Sort();
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence.
		/// If more than (log n) calls to this function are planned in a raw, call Sort() before - costing O(n log n) - for performance in O(log n) instead of O(n).
		/// </summary>
		/// <param name="item">The object to locate in the List. The value can be null for reference types</param>
		/// <returns>The zero-based index of the first occurrence of item, if found; otherwise, a negative value.</returns>
		public int IndexOf(T item)
		{
			if (this.sorted) return Array.BinarySearch<T>(this.data, 0, this.count, item);
			for (var i = 0; i < this.count; i++)
				if (this.data[i].CompareTo(item) == 0) return i;
			return -1;
		}

		/// <summary>
		/// Removes and returns the first item in the heap.
		/// </summary>
		/// <returns>The next value in the heap.</returns>
		public T Remove()
		{
			//if (this.count == 0) throw new InvalidOperationException("Cannot remove item, heap is empty.");
			Debug.Assert(this.count > 0, "Heap is empty!");
			var v = this.data[0];
			this.count--;
			this.data[0] = this.data[this.count];
			this.data[this.count] = default(T);	//Clears the Last Node
			BubbleDown();
			return v;
		}

		/// <summary>
		/// Removes the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		public void RemoveAt(int index)
		{
			var v = this.data[index];
			this.count--;
			this.data[index] = this.data[this.count];
			this.data[this.count] = default(T);
			if ((index != this.count) && (BubbleDown(index) == index))
				BubbleUp(index);
		}

		public void Sort()
		{
			if (this.sorted) return;
			Array.Sort(this.data, 0, this.count);
			this.sorted = true;
		}

		/// <summary>
		/// Set the capacity to the actual number of items (but at least MinimalCapacity), if the current
		/// number of items is less than 90 percent of the current capacity.
		/// </summary>
		public void TrimExcess()
		{
			if ((this.capacity > MinimalCapacity) && (this.count < 0.9 * this.capacity))
				Capacity = this.count;
		}
		#endregion

		#region ICollection<T> and IEnumerable<T> interface
		/// <summary>
		/// Gets whether or not the binary heap is readonly.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Add an item to the heap.
		/// </summary>
		/// <param name="item">The item to add to the heap.</param>
		public void Add(T item)
		{
			if (this.count == this.capacity) Capacity = this.capacity * 2;	//Enlarge the capacity
			this.data[this.count] = item;
			BubbleUp(this.count);
			this.count++;
		}

		/// <summary>
		/// Removes all items from the heap.
		/// </summary>
		public void Clear()
		{
			for (var i = 0; i < this.count; i++)	//TODO: test: "i < this.data.Length" may be in average faster than "i < this.count", due "Range Check Elimination" http://msdn.microsoft.com/en-us/library/ms973858.aspx#highperfmanagedapps_topic10
				this.data[i] = default(T);
			this.count = 0;
		}

		/// <summary>
		/// Checks to see if the binary heap contains the specified item.
		/// See remarks of IndexOf().
		/// </summary>
		/// <param name="item">The item to search the binary heap for.</param>
		/// <returns>A boolean, true if binary heap contains item.</returns>
		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		/// <summary>
		/// Copies the binary heap unsorted to an array at the specified index.
		/// </summary>
		/// <param name="array">One dimensional array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index at which copying begins.</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(this.data, array, this.count);	//TODO: use arrayIndex
		}

		/// <summary>
		/// Gets an enumerator for the binary heap.
		/// </summary>
		/// <returns>An IEnumerator of type T.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < this.count; i++)
				yield return this.data[i];
		}

		/// <summary>
		/// Removes an item from the binary heap. This utilizes the type T's Comparer and will not remove duplicates.
		/// </summary>
		/// <param name="item">The item to be removed.</param>
		/// <returns>Boolean true if the item was removed.</returns>
		public bool Remove(T item)
		{
			var i = IndexOf(item);
			if (i < 0) return false;
			RemoveAt(i);
			return true;
		}
		#endregion

		#region ICollection and IEnumerable interface
		/// <summary>
		/// Gets the number of values in the heap. 
		/// </summary>
		public int Count
		{
			get { return this.count; }
		}

		public void CopyTo(Array array, int index)
		{
			Array.Copy(this.data, array, this.count);	//TODO: use arrayIndex
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this.data; }	//Array.SyncRoot does not exist in Metro
		}
		#endregion

		#region Overriden Linq extensions
		/// <summary>
		/// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence.
		/// </summary>
		/// <param name="match">The Predicate delegate that defines the conditions of the element to search for</param>
		/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
		public int FindIndex(Predicate<T> match)
		{
			var min = default(T);
			var minIndex = -1;
			for (var i = this.count - 1; i >= 0; i--)
			{
				var t = this.data[i];
				if (match(t) && ((minIndex < 0) || (t.CompareTo(min) < 0)))
				{
					min = t;
					minIndex = i;
				}
			}
			return minIndex;
		}

		/// <summary>
		/// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>default(T) if source is empty or if no element passes the test specified by predicate; otherwise, the first element in source that passes the test specified by predicate.</returns>
		public T FirstOrDefault(Func<T, bool> predicate)
		{
			var min = default(T);
			var none = true;
			for (var i = this.count - 1; i >= 0; i--)
			{
				var t = this.data[i];
				if (predicate(t) && (none || (t.CompareTo(min) < 0)))
				{
					min = t;
					none = false;
				}
			}
			return min;
		}
		#endregion
	}
}
