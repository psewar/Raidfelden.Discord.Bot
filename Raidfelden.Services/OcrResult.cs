using System.Collections.Generic;

namespace Raidfelden.Services
{

	public partial class OcrService
	{
		public class OcrResult<T>
		{
			public OcrResult(bool isSuccess, string ocrValue, KeyValuePair<T, double>[] results = null)
			{
				IsSuccess = isSuccess;
				OcrValue = ocrValue;
				Results = results;
			}

			public bool IsSuccess { get; }
			public KeyValuePair<T, double>[] Results { get; }
			public string OcrValue { get; }

			public T GetFirst()
			{
				return Results[0].Key;
			}
		}
	}
}
