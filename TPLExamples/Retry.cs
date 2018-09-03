using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RS.Retry
{
    public interface IRetry
    {
        IRetry HandleException<TException>() where TException : Exception;
        IRetry SuccessChecker(Func<bool> successChecker);
        IRetry AttemptsExhausted(Action attemptsExhuasted);
        IRetry CancellationToken(CancellationToken cancellationToken);
        IRetry Wait(int sec);
        void Run(Action action, int tryCount);
        Task RunAsync(Action action, int tryCount);
    }

    public class Retry : IRetry
    {
        readonly List<Type> _exceptions;

        Func<bool> _successChecker;
        Action _attemptsExhuasted;
        CancellationToken _cancellationToken;
        int _wait;

        public Retry()
        {
            _cancellationToken = new CancellationToken();
            _exceptions = new List<Type>
            {
                typeof(Exception)
            };
            _successChecker = () => { return true; };
            _attemptsExhuasted = () => { };
        }

        public IRetry HandleException<TException>() where TException : Exception
        {
            _exceptions.Remove(typeof(Exception));
            _exceptions.Add(typeof(TException));
            return this;
        }

        public IRetry SuccessChecker(Func<bool> successChecker)
        {
            _successChecker = successChecker;
            return this;
        }

        public void Run(Action action, int tryCount)
        {
            if (tryCount < 1) tryCount = 1;

            int attempt = 0;

            while (tryCount > attempt && !_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    action();

                    if (_successChecker()) { break; }

                    attempt++;
                    Thread.Sleep(_wait * 1000);
                }
                catch (Exception ex)
                {
                    if (!_HandleExcption(ex.GetType()))
                    {
                        throw;
                    }
                    attempt++;
                    Thread.Sleep(_wait * 1000);
                }
            }
            _attemptsExhuasted();
        }

        public IRetry Wait(int sec)
        {
            _wait = sec;
            return this;
        }

        bool _HandleExcption(Type ex)
        {
            return _exceptions.Any(x => x.IsAssignableFrom(ex));
        }

        public IRetry AttemptsExhausted(Action attemptsExhuasted)
        {
            _attemptsExhuasted = attemptsExhuasted;
            return this;
        }

        public IRetry CancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        Task _Run(Action action, int tryCount)
        {
            var task =  new Task(() => { Run(action, tryCount); });
            task.Start();
            return task;
        }

        public async Task RunAsync(Action action, int tryCount)
        {
            await _Run(action, tryCount);
        }
    }
}
