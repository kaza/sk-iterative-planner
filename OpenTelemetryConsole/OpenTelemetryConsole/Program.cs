using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using OpenTelemetry;
using OpenTelemetry.Trace;


namespace OpenTelemetryConsole
{
    internal class Program
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("MyCompany.MyProduct.MyLibrary");

        public static void Main()
        {
            var spanDataList = new List<Activity>();

            var exporter = new InMemoryExporter(spanDataList);

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetSampler(new AlwaysOnSampler())
                .AddSource("MyCompany.MyProduct.MyLibrary")
                .AddProcessor(exporter)
                .Build();

            // Start a new span.
            using (var mainActivity = ActivitySource.StartActivity("Main"))
            {
                mainActivity?.AddEvent(new ActivityEvent("Start of Main Activity"));

                // Simulate some work.
                Thread.Sleep(500);

                // Start a task with nested method invocation.
                var task = Task.Run(() => NestedMethodInvocation());

                // Wait for the task to finish.
                task.Wait();

                mainActivity?.AddEvent(new ActivityEvent("End of Main Activity"));
            }

            // Allow time for the exporter to process.
            Thread.Sleep(1000);
            // Now you can access the collected spans from spanDataList.
            foreach (var activity in spanDataList)
            {
                Console.WriteLine($"SpanId: {activity.SpanId}, ParentId: {activity.ParentSpanId}, Name: {activity.DisplayName}");
                foreach (var ev in activity.Events)
                {
                    Console.WriteLine($"\tEvent: {ev.Name}");
                }
            }

            Console.ReadLine();
        }

        private static void NestedMethodInvocation()
        {
            using (var nestedActivity = ActivitySource.StartActivity("Nested"))
            {
                nestedActivity?.AddEvent(new ActivityEvent("Start of Nested Activity"));

                // Simulate some work.
                Thread.Sleep(500);

                nestedActivity?.AddEvent(new ActivityEvent("End of Nested Activity"));
            }
        }

    }

    public class InMemoryExporter : BaseProcessor<Activity>
    {
        private readonly List<Activity> _spanDataList;

        public InMemoryExporter(List<Activity> spanDataList)
        {
            _spanDataList = spanDataList;
        }

        public override void OnEnd(Activity activity)
        {
            _spanDataList.Add(activity);
        }
    }
}
