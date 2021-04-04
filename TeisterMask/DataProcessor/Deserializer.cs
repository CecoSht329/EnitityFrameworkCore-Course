namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.DataProcessor.ImportDto;
    using TeisterMask.XmlHelper;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            //var projectsXml = XmlConverter.Deserializer<ImportProjectDto>(xmlString, "Projects");
            var mySerializer = new XmlSerializer(typeof(ImportProjectDto[]));
            using var myFileStream = new FileStream("../projects.xml", FileMode.Open);

            var projectsXml = (ImportProjectDto[])mySerializer.Deserialize(myFileStream);

            List<Project> projects = new List<Project>();

            foreach (var projectXml in projectsXml)
            {
                var parsedOpenDate = DateTime.TryParseExact(projectXml.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var openDateProject);

                var parsedDueDate = DateTime.TryParseExact(projectXml.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDateProject);

                if (!IsValid(projectXml) ||
                    projectXml.Tasks.Any(t => !IsValid(t)) ||
                    !parsedOpenDate
                    || openDateProject > dueDateProject)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                Project project = new Project
                {
                    Name = projectXml.Name,
                    OpenDate = openDateProject,
                    DueDate = dueDateProject
                };

                foreach (var taskDto in projectXml.Tasks)
                {
                    var parsedTaskOpenDate = DateTime.TryParseExact(projectXml.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var openDateTask);

                    var parsedTaskDueDate = DateTime.TryParseExact(projectXml.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDateTask);

                    if (!IsValid(taskDto)
                        || !parsedOpenDate
                        || !parsedDueDate
                        || dueDateTask > openDateTask)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }


                    Task task = new Task
                    {
                        Name = taskDto.Name,
                        OpenDate = openDateTask,
                        DueDate = dueDateTask,
                        ExecutionType = taskDto.ExecutionType.Value,
                        LabelType = taskDto.LabelType.Value
                    };
                    project.Tasks.Add(task);
                }
                projects.Add(project);
                sb.AppendLine($"Successfully imported project - {project.Name} with {project.Tasks.Count} tasks.");
            }

            context.Projects.AddRange(projects);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {

            return null;
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}