using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GenCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init();
            DoCreateCode();

            Console.ReadKey();

        }

        public static void DoCreateCode()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string solutionPath = root.Replace(@"GenCode\bin\Debug\net5.0\", "");

            bool loop = true;
            do
            {
                Console.Write("Input Entity Name: ");

                var entity = Console.ReadLine();

                if(entity.ToLower() == "" || entity.ToLower() == "exit")
                {
                    loop = false;
                    continue;
                }

                if (entity == entity.ToLower() || entity == entity.ToUpper())
                {
                    Console.WriteLine("Entity not match upper case and lower case");
                    continue;
                }

                string prefix = "";
                var ans = entity.Split('_');
                if (ans.Length > 1 && ans[0].Length == 2)
                {
                    prefix = ans[0] + @"\";
                }

                string entitypath = solutionPath + @"EPAD_Data\Entities\" + prefix + entity + ".cs";
                if (!File.Exists(entitypath))
                {
                    Console.WriteLine("Entity not exists");
                    continue;
                }

                string repoInterfacePath = solutionPath + @"EPAD_Repository\Interface\" + $"I{entity}Repository.cs";
                string repoImplPath = solutionPath + @"EPAD_Repository\Impl\" + $"{entity}Repository.cs";

                if (File.Exists(repoInterfacePath) || File.Exists(repoImplPath))
                {
                    Console.WriteLine("Exists Repository");
                    continue;
                }

                string serviceInterfacePath = solutionPath + @"EPAD_Services\Interface\" + $"I{entity}Service.cs";
                string serviceImplPath = solutionPath + @"EPAD_Services\Impl\" + $"{entity}Service.cs";

                if (File.Exists(serviceInterfacePath) || File.Exists(serviceImplPath))
                {
                    Console.WriteLine("Exists Service");
                    continue;
                }

                var templateRepoInterface = File.ReadAllText("RepositoryTemplate\\IIC_AttendanceLogRepository.ths");
                var templateRepoImpl = File.ReadAllText("RepositoryTemplate\\IC_AttendanceLogRepository.ths");

                var templateServiceInterface = File.ReadAllText("ServicesTemplate\\IIC_AttendanceLogService.ths");
                var templatServiceImpl = File.ReadAllText("ServicesTemplate\\IC_AttendanceLogService.ths");


                templateRepoInterface = templateRepoInterface.Replace("IIC_AttendanceLogRepository", $"I{entity}Repository");
                templateRepoImpl = templateRepoImpl.Replace("IC_AttendanceLog", entity);

                templateServiceInterface = templateServiceInterface.Replace("IC_AttendanceLog", entity);
                templatServiceImpl = templatServiceImpl.Replace("IC_AttendanceLog", entity);

                try
                {
                    File.WriteAllText(repoInterfacePath, templateRepoInterface);
                    File.WriteAllText(repoImplPath, templateRepoImpl);
                    File.WriteAllText(serviceInterfacePath, templateServiceInterface);
                    File.WriteAllText(serviceImplPath, templatServiceImpl);
                    Console.WriteLine("Success: " + entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            } while (loop);
        }


        public static void Init()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string solutionPath = root.Replace(@"GenCode\bin\Debug\net5.0\", "");


            var allEntitiesFile = Directory.GetFiles(solutionPath + "EPAD_Data\\Entities\\IC");
            List<string> entities = new List<string>();
            foreach (var f in allEntitiesFile)
            {
                FileInfo fi = new FileInfo(f);
                entities.Add(fi.Name.Replace(".cs", ""));
            }

            foreach (var entity in entities)
            {
                if (entity == entity.ToLower() || entity == entity.ToUpper())
                {
                    Console.WriteLine("Entity not match upper case and lower case: " + entity);
                    continue;
                }

                string prefix = "";
                var ans = entity.Split('_');
                if (ans.Length > 1 && ans[0].Length == 2)
                {
                    prefix = ans[0] + @"\";
                }

                string entitypath = solutionPath + @"EPAD_Data\Entities\" + prefix + entity + ".cs";
                if (!File.Exists(entitypath))
                {
                    Console.WriteLine("Entity not exists: " + entity);
                    continue;
                }

                string repoInterfacePath = solutionPath + @"EPAD_Repository\Interface\" + $"I{entity}Repository.cs";
                string repoImplPath = solutionPath + @"EPAD_Repository\Impl\" + $"{entity}Repository.cs";

                if (File.Exists(repoInterfacePath) || File.Exists(repoImplPath))
                {
                    Console.WriteLine("Exists Repository: " + entity);
                    continue;
                }

                string serviceInterfacePath = solutionPath + @"EPAD_Services\Interface\" + $"I{entity}Service.cs";
                string serviceImplPath = solutionPath + @"EPAD_Services\Impl\" + $"{entity}Service.cs";

                if (File.Exists(serviceInterfacePath) || File.Exists(serviceImplPath))
                {
                    Console.WriteLine("Exists Service: " + entity);
                    continue;
                }

                var templateRepoInterface = File.ReadAllText("RepositoryTemplate\\IIC_AttendanceLogRepository.ths");
                var templateRepoImpl = File.ReadAllText("RepositoryTemplate\\IC_AttendanceLogRepository.ths");

                var templateServiceInterface = File.ReadAllText("ServicesTemplate\\IIC_AttendanceLogService.ths");
                var templatServiceImpl = File.ReadAllText("ServicesTemplate\\IC_AttendanceLogService.ths");


                templateRepoInterface = templateRepoInterface.Replace("IIC_AttendanceLogRepository", $"I{entity}Repository");
                templateRepoImpl = templateRepoImpl.Replace("IC_AttendanceLog", entity);

                templateServiceInterface = templateServiceInterface.Replace("IC_AttendanceLog", entity);
                templatServiceImpl = templatServiceImpl.Replace("IC_AttendanceLog", entity);

                try
                {
                    File.WriteAllText(repoInterfacePath, templateRepoInterface);
                    File.WriteAllText(repoImplPath, templateRepoImpl);
                    File.WriteAllText(serviceInterfacePath, templateServiceInterface);
                    File.WriteAllText(serviceImplPath, templatServiceImpl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
