using System;using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace QueryGraphAPI
{
    class UserService
    {
        public static async Task ListUsers(GraphServiceClient graphClient)
        {
            Console.WriteLine("Obtendo lista de usuários...");
         
            var result = await graphClient.Users
                .Request()
                 .Select(e => new
                 {
                     e.DisplayName,
                     e.Id,
                     e.Identities
                 }) 
                .GetAsync();

            foreach (var user in result.CurrentPage)
            {
                Console.WriteLine(JsonConvert.SerializeObject(user));
            }
        }

        public static async Task GetUserById(GraphServiceClient graphClient)
        {
            Console.Write("Insira o ID do objeto do usuário: ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Procurando usuário com ID de objeto '{userId}'...");

            try
            {                
                var result = await graphClient.Users[userId]
                    .Request()
                    .Select(e => new
                    {
                        e.DisplayName,
                        e.Id,
                        e.Identities
                    })
                    .GetAsync();

                if (result != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task GetUserBySignInName(AppSettings config, GraphServiceClient graphClient)
        {
            Console.Write("Insira o nome de login do usuário (nome de usuário ou endereço de e-mail): ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Procurando usuário com nome de login '{userId}'...");

            try
            {                
                var result = await graphClient.Users
                    .Request()
                    .Filter($"identities/any(c:c/issuerAssignedId eq '{userId}' and c/issuer eq '{config.TenantId}')")
                    .Select(e => new
                    {
                        e.DisplayName,
                        e.Id,
                        e.Identities
                    })
                    .GetAsync();

                if (result != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task DeleteUserById(GraphServiceClient graphClient)
        {
            Console.Write("Insira o ID do objeto do usuário: ");
            string userId = Console.ReadLine();

            Console.WriteLine($"Procurando usuário com ID de objeto '{userId}'...");

            try
            {                
                await graphClient.Users[userId]
                   .Request()
                   .DeleteAsync();

                Console.WriteLine($"User with object ID '{userId}' successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task SetPasswordByUserId(GraphServiceClient graphClient)
        {
            Console.Write("Insira o ID do objeto do usuário: ");
            string userId = Console.ReadLine();

            Console.Write("Insira a nova senha: ");
            string password = Console.ReadLine();

            Console.WriteLine($"Procurando usuário com ID de objeto '{userId}'...");

            var user = new User
            {
                PasswordPolicies =  "DisablePasswordExpiration,DisableStrongPassword",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = password,
                }
            };

            try
            {                
                await graphClient.Users[userId]
                   .Request()
                   .UpdateAsync(user);

                Console.WriteLine($"Usuário com ID de objeto '{userId}' atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task BulkCreate(AppSettings config, GraphServiceClient graphClient)
        {            
            string appDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(appDirectoryPath, "data/" + config.UsersFileName);
            
            if (!System.IO.File.Exists(dataFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Arquivo '{dataFilePath}' não encontrado.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Iniciando operação de criação em massa...");
            
            UsersModel users = UsersModel.Parse(System.IO.File.ReadAllText(dataFilePath));

            foreach (var user in users.Users)
            {
                user.SetB2CProfile(config.TenantId);              

                try
                {                    
                    User user1 = await graphClient.Users
                                    .Request()
                                    .AddAsync(user);

                    Console.WriteLine($"Usuario '{user.DisplayName}' criado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}