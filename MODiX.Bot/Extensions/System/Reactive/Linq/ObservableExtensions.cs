namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        public static IObservable<T> DoOnError<T>(this IObservable<T> source, Action<Exception> onError)
            => Observable.Create<T>(observer => source.Subscribe(
                observer.OnNext,
                ex =>
                {
                    onError.Invoke(ex);
                    observer.OnError(ex);
                },
                observer.OnCompleted));

        public static IObservable<T> Throw<T>(this IObservable<Exception> source)
            => source.Select<Exception, T>(ex => throw ex);

        public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source)
                where T : struct
            => source
                .Where(value => value.HasValue)
                .Select(value => value!.Value);
    }
}
