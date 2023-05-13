using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Experiments;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Web;
using Microsoft.SemanticKernel.Skills.Web.Bing;
using Microsoft.SemanticKernel.Skills.Web.Google;
using Microsoft.SemanticKernel.Text;
using NCalc;

namespace UsageExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            await LeonardosGirflfriend();
        }

        private static async Task LeonardosGirflfriend()
        {
            var kernel = GetKernel();
            using var googleConnector = new GoogleConnector(Env.Var("GOOGLE_API_KEY"), Env.Var("GOOGLE_SEARCH_ENGINE_ID"));
            var webSearchEngineSkill = new WebSearchEngineSkill(googleConnector);
            kernel.ImportSkill(webSearchEngineSkill, "google");
            kernel.ImportSkill(new LanguageCalculatorSkill(kernel), "calculator");

            
            string goal = "Who is Leo DiCaprio's girlfriend? What is her current age raised to the 0.43 power?";
            //string goal =  "Who is the current president of the United States? What is his current age divided by 2";
            IterativePlanner planer = new IterativePlanner(kernel);
            var plan =await  planer.ExecutePlanAsync(goal);
          
        }

        private static IKernel GetKernel()
        {
            var kernel = new KernelBuilder()
                .WithLogger(ConsoleLogger.Log)
                .Build();

            kernel.Config.AddAzureTextCompletionService(
                Env.Var("AZURE_OPENAI_DEPLOYMENT_NAME"),
                Env.Var("AZURE_OPENAI_ENDPOINT"),
                Env.Var("AZURE_OPENAI_KEY")
            );

            return kernel;

        }
    }
}