using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SaberTask2
{
    class Program
    {
        static int _possibility; //вероятность, заданная пользователем
        static int _curStep; //на каком номере броска(подряд) мы сейчас
        static int _thisChance; //номер удачного выпадения в этом блоке
        static int _nextChance; //номер удачного выпадения в след блоке
        static int _koef; //коэффициент разноса выпадания в двух блоках
        static int _step; //длина одного блока

        static string _filename = Path.Combine(Directory.GetCurrentDirectory(), "help_file.txt");
        //вспомогательный файл, в котором хранится инфа между запусками. В жизни вынесен со всеми доп данными в скрытую директорию. Но сейчас он тут

        static Random random = new Random(DateTime.Now.Millisecond);

        static void Main(string[] args)
        {
            if (!TryReadFile())
            {
                Console.WriteLine("Не задана вероятность выпадения сундука. ");
                ReadDataFromConsole();
            }
            else
            {
                Console.Write("Найдена вероятность выпадения сундука " + _possibility + "%. Если желаете ее изменить, нажмите 1.\n" +
                    "Если желаете оставить старую вероятность, то нажмите 2: ");
                var input = Console.ReadKey();
                Console.WriteLine(); // для красивого вывода
                if (input.Key == ConsoleKey.D1)
                    ReadDataFromConsole();
            }

            while (true)
            {
                Console.Write("Нажмите enter, чтобы сделать бросок и поймать ТОТ САМЫЙ сундук!");
                Console.ReadLine();
                OneChance();
            }
        }

        static private bool TryReadFile()
        {
            if (!File.Exists(_filename))
                return false;

            var res = new List<int>();
            File.ReadAllText(_filename).Split('\n', '\t').ToList().ForEach(x => res.Add(int.Parse(x)));
            _possibility = res[0];
            _curStep = res[1];
            _thisChance = res[2];
            _nextChance = res[3];
            _step = 100 / _possibility;

            // делим на 3, чтобы пользователю было тяжелее вычислить логику. Можно использовать все, что угодно(в разумных пределах)
            _koef = _step / 3;
            return true;
        }

        static private void WriteFile()
        {
            File.WriteAllText(_filename, String.Format("{0}\n{1}\n{2}\t{3}", _possibility, _curStep, _thisChance, _nextChance));
            /*  Структура файла:
             * Вероятность
             * На каком шаге в блоке мы сейчас
             * Номер шага с выпадением в этом блоке      Номер шага с выпадением в след блоке
             */
        }

        static private void ReadDataFromConsole(string line = "Введите значение вероятности выпадения в %: ")
        {
            Console.Write(line);
            //ввод вероятности
            var input = Console.ReadLine();
            var possibility = -1f;
            //проверка корректности. Ждем правильный ввод
            while (!float.TryParse(input, out possibility) || float.TryParse(input, out possibility) && (possibility <= 0 || possibility >= 100))
            {
                Console.WriteLine("\nОжидается число, большее 0 и меньшее 100! Попробуйте еще раз...");
                Console.Write(line);
                input = Console.ReadLine();
            }

            _possibility = (int)MathF.Ceiling(possibility);
            if (_possibility == 100)
                _possibility = 99;

            _curStep = 0;

            // ВЫзываем рассчет случайных чисел в блоке.
            CountPossibilities();
        }

        // прекрасный главный алгоритм
        static private void CountPossibilities()
        {
            _step = 100 / _possibility;

            // делим на 3, чтобы пользователю было тяжелее вычислить логику. Можно использовать все, что угодно(в разумных пределах)
            _koef = _step / 3;

            _thisChance = random.Next(_koef, _step - _koef);
            // границы _koef задаются специально, чтобы игрок не получил редкий сундук в первый или последний бросок, а где-то в серединке
            
            CountSecondBlock();
        }

        //и его вторая важная часть
        static private void CountSecondBlock()
        {
            var nextChance = _step + random.Next(_koef, _step - _koef);
            while (nextChance - _thisChance < _koef || nextChance - _thisChance > _step + _koef) // считаем во втором блоке
            {
                nextChance = _step + random.Next(_koef, _step - _koef);
            }

            _nextChance = nextChance - _step;
        }

        static private void GoToNextBlock()
        {
            _thisChance = _nextChance;
            _curStep = 0;
            CountSecondBlock();
        }

        static private void OneChance()
        {
            Console.Write(String.Format("({0})\t", _curStep + 1));
            if (_curStep == _thisChance)
                Console.WriteLine("Поздравляем! Вы выиграли редкий сундук!");
            else
                Console.WriteLine("Упс, это обычный сундук. Попытайтесь еще раз и Вам обязательно повезет!");

            _curStep++;
            if (_curStep == _step)
                GoToNextBlock();

            WriteFile();
        }
    }
}
