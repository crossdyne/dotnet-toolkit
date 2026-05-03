namespace Crossdyne.Toolkit.Results
{
    public interface IResultWithFactory<TSelf> where TSelf : Result
    {
        static abstract TSelf CreateFailure(IEnumerable<Error> errors);
    }
}