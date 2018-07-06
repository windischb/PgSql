using System;
using System.Collections.Generic;
using System.Text;

namespace doob.PgSql.Listener
{
    class XAObservable<T> : IObservable<T>
    {
        private IObservable<T> obs;

        

        public IDisposable Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }
}
