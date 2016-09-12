using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace ReactiveConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Demo.Subject();
            //Demo.ReplaySubject();
            //Demo.SubjectOnCompleted();
            //Demo.ObservableCreateSimple().Subscribe(x => Console.WriteLine(x),()=> Console.WriteLine("We have completed an observal"));

            //using (Demo.Interval())
            //{
            //    Console.WriteLine("Press any key to unsubscribe");
            //    Console.ReadKey();
            //}

            //Demo.Timer();

            Demo.Mouse();

            Console.WriteLine("Press any key to end application");
            Console.ReadKey(true);
        }






    }

    static class Demo
    {
        public static void Subject()
        {
            var subject = new Subject<string>();

            subject.OnNext("a");
            subject.OnNext("b");
            subject.Subscribe(Console.WriteLine);
            subject.OnNext("c");

        }

        public static void ReplaySubject()
        {
            var bufferSize = 2;
            var subject = new ReplaySubject<string>(bufferSize);
            subject.OnNext("a");
            subject.OnNext("b");
            subject.OnNext("c");
            subject.Subscribe(Console.WriteLine);
            subject.OnNext("d");
        }

        public static void SubjectOnCompleted()
        {
            var subject = new Subject<int>();
            subject.Subscribe(
                Console.WriteLine,
                () => Console.WriteLine("Completed"));
            subject.OnNext(1);
            subject.OnCompleted();
            subject.OnNext(2);
        }


        /// <summary>
        /// Lets create IObservable SOURCE of data. 
        /// 
        /// The Create factory method is the preferred way to implement custom observable sequences. The usage of subjects should largely remain in the realms of samples and testing. 
        /// Subjects are a great way to get started with Rx. They reduce the learning curve for new developers, however they pose several concerns that the Create method eliminates. 
        /// Rx is effectively a functional programming paradigm. Using subjects means we are now managing state, which is potentially mutating. Mutating state and asynchronous programming are very hard to get right. 
        /// Furthermore many of the operators (extension methods) have been carefully written to ensure correct and consistent lifetime of subscriptions and sequences are maintained.
        ///  When you introduce subjects you can break this. Future releases may also see significant performance degradation if you explicitly use subjects.
        /// 
        ///The Create method is also preferred over creating custom types that implement the IObservable interface. There really is no need to implement the 
        /// observer/observable interfaces yourself.Rx tackles the intricacies that you may not think of such as thread safety of notifications and subscriptions.
        /// </summary>
        /// <returns></returns>
        public static IObservable<string> ObservableCreateSimple()
        {
            return Observable.Create<string>(
             (IObserver<string> observer) =>
            {
                observer.OnNext("a");
                observer.OnNext("b");
                observer.OnCompleted();



                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed") /* DO SOMETHING  WHILE WE ARE DISPOSING subscribe*/);


                /** IF DON'T WANT TO DISPOSE ANYTHING
                 * return Disposable.Empty;
                 */
            });
        }


        public static IDisposable Interval()
        {
            var interval = Observable.Interval(TimeSpan.FromMilliseconds(250));
            return interval.Subscribe(
            Console.WriteLine,
            () => Console.WriteLine("completed"));
        }

        public static void Timer()
        {
            var timer = Observable.Timer(TimeSpan.FromSeconds(1));
            timer.Subscribe(
            Console.WriteLine,
            () => Console.WriteLine("completed"));
        }

        public static void Mouse()
        {

            var m_GlobalHook = Hook.GlobalEvents();
            var isLeftDown = false;
            Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                h => m_GlobalHook.MouseDown += h,
                h => m_GlobalHook.MouseDown -= h).Subscribe(o =>
                {
                    isLeftDown = (o.EventArgs.Button & MouseButtons.Left) != 0;
                });
            Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                h => m_GlobalHook.MouseUp += h,
                h => m_GlobalHook.MouseUp -= h).Subscribe(o =>
                {
                    isLeftDown = (o.EventArgs.Button & MouseButtons.Left) == 0;
                });

            Observable.FromEventPattern<MouseEventExtArgs>(
                h => m_GlobalHook.MouseMoveExt += h, 
                h => m_GlobalHook.MouseMoveExt -= h)
                .Select(x => x.EventArgs.Location)
                .Where(x => isLeftDown)
                //.Timestamp()
                //.Throttle()
                .Buffer(TimeSpan.FromMilliseconds(500))
                
                //.SelectMany(x => x)
                .Subscribe(args =>
                {
                    
                    Console.WriteLine(args.Count);



                });

            var f = new Form();
            Application.Run(f);


        }

    }



}
