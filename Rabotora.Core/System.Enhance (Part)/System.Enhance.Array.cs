namespace System.Enhance.Array
{
	public static class ArrayHelper
	{
		public static int IndexOfSubByteArray(this byte[] fatherArray, byte[] findArray)
		{
			int index = -1;
			int subIndex = 0;
			for (int i = 0; i < fatherArray.Length; i++)
			{
				if (fatherArray[i] == findArray[subIndex])
				{
					subIndex++;
				}
				else
				{
					subIndex = 0;
					continue;
				}
				if (subIndex + 1 == findArray.Length)
				{
					index = i - subIndex;
					break;
				}
			}
			return index;
		}

		public static bool DeepEquals(this byte[] array1, byte[] array2)
		{
			if (array1.Length == array2.Length)
			{
				for (int i = 0; i < array1.Length; i++)
				{
					if (array1[i] == array2[i])
					{
						continue;
					}
					else
					{
						return false;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}