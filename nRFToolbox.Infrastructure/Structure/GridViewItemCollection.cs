using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NRFToolbox.Infrastructure
{
		public class GridViewItemCollection<T> : IEnumerable<T>
		{
			private ObservableCollection<T> itemCollection = new ObservableCollection<T>();

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return itemCollection.GetEnumerator();
			}

			public void Add(T item)
			{
				itemCollection.Add(item);
			}
		}
}
