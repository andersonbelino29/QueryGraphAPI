using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace QueryGraphAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private static async Task RunAsync()
        {

            AppSettings config = AppSettingsFile.ReadFromJsonFile();


            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(config.AppId)
                .WithTenantId(config.TenantId)
                .WithClientSecret(config.ClientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);


            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            Program.PrintCommands();

            try
            {
                while (true)
                {

                    Console.Write("Digite o comando e pressione ENTER: ");
                    string decision = Console.ReadLine();
                    switch (decision.ToLower())
                    {
                        case "1":
                            await UserService.ListUsers(graphClient); ;
                            break;
                        case "2":
                            await UserService.GetUserById(graphClient); ;
                            break;
                        case "3":
                            await UserService.GetUserBySignInName(config, graphClient); ;
                            break;
                        case "4":
                            await UserService.DeleteUserById(graphClient);
                            break;
                        case "5":
                            await UserService.SetPasswordByUserId(graphClient);
                            break;
                        case "6":
                            await UserService.BulkCreate(config, graphClient);
                            break;
                        case "help":
                            Program.PrintCommands();
                            break;
                        case "exit":
                            return;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Comando inválido. Digite 'help' para mostrar uma lista de comandos.");
                            Console.ResetColor();
                            break;
                    }

                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    while (innerException != null)
                    {
                        Console.WriteLine(innerException.Message);
                        innerException = innerException.InnerException;
                    }
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                Console.ResetColor();
            }

            Console.ReadLine();
        }

        private static void PrintCommands()
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Descrição do comando");
            Console.WriteLine("====================");
            Console.WriteLine("[1]      Obtenha todos os usuários (uma página)");
            Console.WriteLine("[2]      Obtenha o usuário por ID do objeto");
            Console.WriteLine("[3]      Obtenha o usuário pelo nome de login");
            Console.WriteLine("[4]      Excluir usuário por ID de objeto");
            Console.WriteLine("[5]      Atualizar senha do usuário");
            Console.WriteLine("[6]      Criar usuários (importação em massa)");
            Console.WriteLine("[help]   Mostrar comandos disponíveis");
            Console.WriteLine("[exit]   Saia do programa");
            Console.WriteLine("-------------------------");
        }
    }
}
