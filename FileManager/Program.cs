using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    class Program
    {
        private static string currentDir = Properties.Settings.Default.CurrentDirectory;
        private static StringBuilder historyCommand = new StringBuilder(128);

        /// <summary>
        /// Отрисовка окна
        /// </summary>
        /// <param name="x">Начальная координата по оси X</param>
        /// <param name="y">Начальная координата по оси Y</param>
        /// <param name="width">Ширина окна</param>
        /// <param name="height">Высота окна</param>
        static void DrawWindow(int x, int y, int width, int height, bool topIntermediate, bool bottomIntermediate)
        {
            //Верхняя граница
            Console.SetCursorPosition(x, y);
            if (topIntermediate)
            {
                Console.Write("╠");
            }
            else
            {
                Console.Write("╔");
            }
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            if (topIntermediate)
            {
                Console.Write("╣");
            }
            else
            {
                Console.Write("╗");
            }

            //Левая и праваяграница
            Console.SetCursorPosition(x, y + 1);
            for (int i = 0; i < height - 1; i++)
            {
                Console.Write("║");
                for (int j = 0; j < width - 2; j++)
                {
                    Console.Write(" ");
                }
                Console.Write("║");
                Console.SetCursorPosition(x, y + 1 + i);
            }

            //Нижняя граница
            if (bottomIntermediate)
            {
                Console.Write("╠");
            }
            else
            {
                Console.Write("╚");
            }
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            if (bottomIntermediate)
            {
                Console.Write("╣");
            }
            else
            {
                Console.Write("╝");
            }

            Console.SetCursorPosition(x + 1, y + 1);
        }

        /// <summary>
        /// Получение текущего положение курсора
        /// </summary>
        /// <returns></returns>
        static (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft, Console.CursorTop);
        }

        /// <summary>
        /// Отрисовывает главное окно по умолчанию
        /// </summary>
        static void DrawDefaultMain()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, 0, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.MainWindowHeight, false, true);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Отрисовывает окно информации по умолчанию
        /// </summary>
        static void UpdateInfo()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, Properties.Settings.Default.MainWindowHeight - 1, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.InfoWindowHeight, true, true);
            Console.ForegroundColor = ConsoleColor.White;
            (int left, int top) = GetCursorPosition();
            Console.Write(" Доступные команды:");
            Console.SetCursorPosition(left, top + 1);
            Console.Write(" Сменить текущую директорию: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("cd <Путь директории>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(left, top + 2);
            Console.Write(" Вывести древо каталогов и файлов: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("ls <Путь директории> <Номер страницы>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(left, top + 3);
            Console.Write(" Скопировать файл или каталог: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("cp <Путь исходника> <Путь назначения>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(left, top + 4);
            Console.Write(" Удалить файл или каталог: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("rm <Путь файла/каталога>");
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(left, top + 5);
            Console.Write(" Вывести информацию о файле или каталоге: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("info <Путь файла/каталога>");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Отрисовывает окно информации с сообщением
        /// </summary>
        /// <param name="message"></param>
        static void UpdateInfo(string message, bool title)
        {
            int maxLines = Properties.Settings.Default.InfoWindowHeight - 2;
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, Properties.Settings.Default.MainWindowHeight - 1, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.InfoWindowHeight, true, true);
            Console.ForegroundColor = ConsoleColor.White;
            (int left, int top) = GetCursorPosition();
            string[] lines = message.Split('\n');
            for (int i = 0; i < maxLines; i++)
            {
                if (i < lines.Length)
                {
                    if (title && i == 0)
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(lines[i]);
                    if (title && i == 0)
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(left, top + i + 1);
                }
            }
        }

        /// <summary>
        /// Обновляет строку ввода, а также запускает методы для обработки команд
        /// </summary>
        static void CleanInputConsole()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, Properties.Settings.Default.MainWindowHeight + Properties.Settings.Default.InfoWindowHeight - 2, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.InputWindowHeight, true, false);
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write(currentDir + ">");
        }

        /// <summary>
        /// Обновляет строку ввода, а также запускает методы для обработки команд
        /// </summary>
        static void UpdateInputConsole()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, Properties.Settings.Default.MainWindowHeight + Properties.Settings.Default.InfoWindowHeight - 2, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.InputWindowHeight, true, false);
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write(currentDir + ">");
            ProcessEnterCommand(Properties.Settings.Default.WindowWidth - 2);
        }

        /// <summary>
        /// Выводит все файлы
        /// </summary>
        /// <param name="subFiles">Массив файлов в формате FileInfo</param>
        /// <param name="indent">Отступ перед выводом файлов</param>
        static void AddAllFiles(StringBuilder tree, FileInfo[] subFiles, string indent)
        {
            for (int i = 0; i < subFiles.Length; i++)
            {
                tree.Append(indent);
                tree.Append((i == subFiles.Length - 1) ? "└" : "├");
                tree.Append(subFiles[i].Name + "\n");
            }
        }

        /// <summary>
        /// Построение дерева каталогов и файлов
        /// </summary>
        /// <param name="tree">Строка, записывающая всё дерево каталогов и файлов</param>
        /// <param name="dir">Путь директории, по которой необходимо построить дерево</param>
        /// <param name="indent">Отступ (при первом вызове необходимо писать "")</param>
        /// <param name="lastDirectory">Является ли текущая директория конечной (при первом вызове необходимо писать "true")</param>
        static void GetTree(StringBuilder tree, DirectoryInfo dir, string indent, bool lastDirectory)
        {
            tree.Append(indent);
            tree.Append(lastDirectory ? "└" : "├");
            tree.Append(dir.Name + "\n");

            indent += lastDirectory ? " " : "│ ";

            FileInfo[] subFiles = dir.GetFiles();
            DirectoryInfo[] subDirs = dir.GetDirectories();
            for (int i = 0; i < subDirs.Length; i++)
            {
                GetTree(tree, subDirs[i], indent, ((i == subDirs.Length - 1) && (subFiles.Length == 0)));
            }
            AddAllFiles(tree, subFiles, indent);
        }

        /// <summary>
        /// Отрисовка дерева каталогов и файлов постранично
        /// </summary>
        /// <param name="dir">Директория, которую необходимо отрисовать</param>
        /// <param name="page">Номер страницы дерева каталогов и файлов, которую необходимо вывести</param>
        static void DrawTree(DirectoryInfo dir, int page)
        {
            StringBuilder tree = new StringBuilder();
            GetTree(tree, dir, "", true);
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawWindow(0, 0, Properties.Settings.Default.WindowWidth, Properties.Settings.Default.MainWindowHeight, false, true);
            Console.ForegroundColor = ConsoleColor.White;
            (int currentLeft, int currentTop) = GetCursorPosition();
            int pageLines = Properties.Settings.Default.PageLines;
            string[] lines = tree.ToString().Split('\n');
            int totalPage = (lines.Length + pageLines - 1) / pageLines;
            if (page > totalPage)
            {
                page = totalPage;
            }
            else if (page <= 0)
            {
                page = 1;
            }
            for (int i = (pageLines * (page - 1)), counter = 0; i < (pageLines * page); i++, counter++)
            {
                if (i < lines.Length - 1)
                {
                    Console.SetCursorPosition(currentLeft, currentTop + counter);
                    Console.WriteLine(lines[i]);
                }
            }
            string infoPage = page + " из " + totalPage;
            Console.SetCursorPosition(Properties.Settings.Default.WindowWidth - infoPage.Length - 2, currentTop);
            Console.Write(infoPage);
        }

        /// <summary>
        /// Копирование каталога в другой каталог
        /// </summary>
        /// <param name="sourceDir">Исходный каталог, содержимое которого копируется</param>
        /// <param name="destinationDir">Конечный каталог, в который происходит копирование. Чтобы копирование происходило в каталог с названием исходного, необходимо в конце пути добавить его имя.</param>
        static void CopyCatalog(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Исходный каталог не найден: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Создаёт текущий каталог в папке назначения
            Directory.CreateDirectory(destinationDir);

            // Файлы в исходном каталоге копируются в конечный
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }


            //На каждую папку в текущем каталоге вызывается этот же метод рекурсивно
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyCatalog(subDir.FullName, newDestinationDir);
            }
        }

        /// <summary>
        /// Копирование каталога в другой каталог
        /// </summary>
        /// <param name="sourceDir">Исходный каталог, содержимое которого копируется</param>
        /// <param name="destinationDir">Конечный каталог, в который происходит копирование. Чтобы копирование происходило в каталог с названием исходного, необходимо в конце пути добавить его имя.</param>
        static void DeleteCatalog(string sourceDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Исходный каталог не найден: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Каждый файл в текущем каталоге удаляется
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            //На каждую папку в текущем каталоге вызывается этот же метод рекурсивно
            foreach (DirectoryInfo subDir in dirs)
            {
                DeleteCatalog(subDir.FullName);
            }

            // После удаления предыдущих каталогов и файлов (если есть), удаляется текущий каталог
            Directory.Delete(sourceDir);
        }

        /// <summary>
        /// Формирует сведения о файле и запрашивает их отображение в окне информации
        /// </summary>
        /// <param name="file">Путь файла</param>
        static void PrintFileInfo(string file)
        {
            FileInfo fileInfo = new FileInfo(@file);
            UpdateInfo($"Информация о файле\nПуть: {fileInfo.FullName}\nИмя: {fileInfo.Name}\nРазмер: {fileInfo.Length}\nВремя создания: {fileInfo.CreationTime}", true);
        }

        /// <summary>
        /// Формирует сведения о директории и запрашивает их отображение в окне информации
        /// </summary>
        /// <param name="file">Путь директории</param>
        static void PrintDirectoryInfo(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(@dir);
            UpdateInfo($"Информация о директории\nПуть: {dirInfo.FullName}\nИмя: {dirInfo.Name}\nВремя последнего изменения: {dirInfo.LastWriteTime}\nВремя создания: {dirInfo.CreationTime}", true);
        }

        /// <summary>
        /// Обработка консольных команд
        /// </summary>
        /// <param name="command">Строка команды</param>
        static void ParseCommandString(string command)
        {
            string[] commandParams = command.Split(' ');
            if (commandParams.Length == 0)
            {
                return;
            }
            try
            {
                switch (commandParams[0].ToLower())
                {
                    //Перейти в другую директорию
                    case "cd":
                        if (commandParams.Length > 1)
                        {
                            if (Directory.Exists(commandParams[1]))
                            {
                                Properties.Settings.Default.CurrentDirectory = commandParams[1];
                                currentDir = Properties.Settings.Default.CurrentDirectory;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                throw new Exception("Указанной директории не существует.");
                            }
                        }
                        UpdateInfo();
                        break;

                    //Скопировать файл/каталог в другой файл/каталог
                    case "cp":
                        if (commandParams.Length > 1)
                        {
                            if (File.Exists(commandParams[1]))
                            {
                                new FileInfo(@commandParams[1]).CopyTo(@commandParams[2]);
                            }
                            else if (Directory.Exists(commandParams[1]) && Directory.Exists(commandParams[2]))
                            {
                                string newDestinationDir = Path.Combine(commandParams[2], new DirectoryInfo(@commandParams[1]).Name);
                                CopyCatalog(commandParams[1], newDestinationDir);
                            }
                            else
                            {
                                throw new Exception("Вы не указали существующий файл или каталог.");
                            }
                        }
                        UpdateInfo();
                        break;

                    //Скопировать файл/каталог в другой файл/каталог
                    case "rm":
                        if (commandParams.Length > 1)
                        {
                            if (File.Exists(commandParams[1]))
                            {
                                new FileInfo(@commandParams[1]).Delete();
                            }
                            else if (Directory.Exists(commandParams[1]))
                            {
                                DeleteCatalog(commandParams[1]);
                            }
                            else
                            {
                                throw new Exception("Вы не указали существующий файл или каталог.");
                            }
                        }
                        UpdateInfo();
                        break;

                    //Скопировать файл/каталог в другой файл/каталог
                    case "info":
                        if (commandParams.Length > 1)
                        {
                            if (File.Exists(commandParams[1]))
                            {
                                PrintFileInfo(commandParams[1]);
                            }
                            else if (Directory.Exists(commandParams[1]))
                            {
                                PrintDirectoryInfo(commandParams[1]);
                            }
                            else
                            {
                                throw new Exception("Вы не указали существующий файл или каталог.");
                            }
                        }
                        break;

                    //Отобразить дерево каталога
                    case "ls":
                        if (commandParams.Length == 1)
                        {
                            DrawTree(new DirectoryInfo(@currentDir), 1);
                        }
                        else if (commandParams.Length == 2)
                        {
                            if (int.TryParse(commandParams[1], out int page))
                            {
                                DrawTree(new DirectoryInfo(@currentDir), page);
                            }
                            else if (Directory.Exists(commandParams[1]))
                            {
                                DrawTree(new DirectoryInfo(@commandParams[1]), 1);
                            }
                            else
                            {
                                throw new Exception("Вы не указали существующий каталог или номер страницы.");
                            }
                        }
                        else if (commandParams.Length == 3)
                        {
                            if (Directory.Exists(commandParams[1]) && int.TryParse(commandParams[1], out int page))
                            {
                                DrawTree(new DirectoryInfo(@commandParams[1]), page);
                            }
                            else
                            {
                                throw new Exception("Вы не указали существующий каталог или указали неправильный номер страницы.");
                            }
                        }
                        else
                        {
                            throw new Exception("Команда не распознана.");
                        }
                        UpdateInfo();
                        break;

                    default:
                        UpdateInfo();
                        break;
                }
            }
            catch (Exception e)
            {
                UpdateInfo(e.Message, false);

                DateTime thisDay = DateTime.Now;
                string pathError = Path.Combine(Directory.GetCurrentDirectory() + "\\errors\\");
                if (!Directory.Exists(pathError))
                {
                    Directory.CreateDirectory(pathError);
                }
                File.AppendAllText(Path.Combine(pathError + "random_name_exception.txt"), thisDay + ": " + e.Message + "\n");
            }
            UpdateInputConsole();
        }

        /// <summary>
        /// Обработка процесса ввода команды пользователем.
        /// </summary>
        /// <param name="width">Максимальная ширина строки для ввода</param>
        /// <returns></returns>
        static bool ProcessEnterCommand(int width)
        {
            (int left, int top) = GetCursorPosition();
            StringBuilder command = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            char key;

            string[] oldCommands = historyCommand.ToString().Split('\n');
            int currentCommand = 0;

            do
            {
                (int currentLeft, int currentTop) = GetCursorPosition();
                keyInfo = Console.ReadKey(true);
                key = keyInfo.KeyChar;

                if (keyInfo.Key != ConsoleKey.Enter && key != '\0' && currentLeft < width)
                {
                    command.Append(key);
                    Console.Write(key);
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (command.Length > 0)
                    {
                        command.Remove(command.Length - 1, 1);
                    }
                    if (currentLeft > left)
                    {
                        Console.SetCursorPosition(currentLeft - 1, top);
                        Console.Write(" ");
                        Console.SetCursorPosition(currentLeft - 1, top);
                    }
                    else
                    {
                        command.Clear();
                        Console.SetCursorPosition(left, top);
                    }
                }

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (currentCommand < oldCommands.Length - 1)
                    {
                        currentCommand++;
                        if (oldCommands[currentCommand] != "")
                        {
                            command.Clear();
                            command.Append(oldCommands[currentCommand]);
                            CleanInputConsole();
                            Console.Write(oldCommands[currentCommand]);
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (currentCommand == 1)
                    {
                        currentCommand--;
                        command.Clear();
                        CleanInputConsole();
                    }
                    if (currentCommand > 1)
                    {
                        currentCommand--;
                        command.Clear();
                        command.Append(oldCommands[currentCommand]);
                        CleanInputConsole();
                        Console.Write(oldCommands[currentCommand]);
                    }
                }
                else
                {
                    currentCommand = 0;
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);
            if (command.Length > 0)
            {
                historyCommand.Append("\n" + command);
            }
            ParseCommandString(command.ToString());

            return true;
        }

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Title = "File Manager";

            Console.SetWindowSize(Properties.Settings.Default.WindowWidth, Properties.Settings.Default.WindowHeight);
            Console.SetBufferSize(Properties.Settings.Default.WindowWidth, Properties.Settings.Default.WindowHeight);

            DrawDefaultMain();
            UpdateInfo();
            UpdateInputConsole();
        }
    }
}
