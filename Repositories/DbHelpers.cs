using GenerateDishesAPI.Data;
using Microsoft.EntityFrameworkCore;
using GenerateDishesAPI.Models;

namespace GenerateDishesAPI.Repositories
{
    public class DbHelpers
    {
        public bool LoginCheck() 
        {
            using (var context = new ApplicationContext())
            {
                bool loginCheck = false;

                    var Users = context.Users.ToList();
                    Console.WriteLine("Enter your email:");
                    string userEmail = Console.ReadLine();
                    Console.WriteLine("Enter your password:");
                    string userPassword = Console.ReadLine();
                    Console.Clear();

                    var MatchingUser = Users.Select(x => x).Where(x => x.Email == userEmail && x.Password == userPassword).ToList();
                    if (MatchingUser.Count == 0)
                    {
                        Console.WriteLine("Incorrect email or password");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Logged in succesfully");
                        loginCheck = true;
                        return loginCheck;
                    }

            }
        }

        public void CreateAccount()
        {
            
            using(var context = new ApplicationContext())
            {
                Console.WriteLine("Create new account");
                Console.WriteLine("Firstname:");
                string firstname = Console.ReadLine();
                Console.WriteLine("Lastname:");
                string lastname = Console.ReadLine();
                Console.WriteLine("Email:");
                string email = Console.ReadLine();
                Console.WriteLine("Password:");
                string password = Console.ReadLine();
                Console.Clear();

                var createdUser = new User();
                createdUser.FirstName = firstname;
                createdUser.LastName = lastname;
                createdUser.Email = email;
                createdUser.Password = password;

                context.Add(createdUser);
                context.SaveChanges();
                Console.WriteLine($"User Added {firstname} {lastname} Email: {email} Password: {password}");
            } 
        }
    }
}
