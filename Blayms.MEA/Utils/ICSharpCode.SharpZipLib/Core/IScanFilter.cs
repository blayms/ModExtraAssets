namespace Blayms.MEA.Utils.ICSharpCode.SharpZipLib.Core
{
    /// <summary>
    /// Scanning filters support filtering of names.
    /// </summary>
    internal interface IScanFilter
	{
		/// <summary>
		/// Test a name to see if it 'matches' the filter.
		/// </summary>
		/// <param name="name">The name to test.</param>
		/// <returns>Returns true if the name matches the filter, false if it does not match.</returns>
		bool IsMatch(string name);
	}
}
