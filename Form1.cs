using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace Predictor
{
    public partial class Form1 : Form
    {
        private const string APP_NAME = "PREDICTOR";
        //В настройках свойств файла 'predictionsConfig.json' устанавливаем настройки, чтобы файл сохранялся в том же месте что и .exe 
        //Environment.CurrentDirectory определяет путь .exe файла 
        private readonly string PREDICTIONS_CONFIG_PATH = $"{Environment.CurrentDirectory}\\predictionsConfig.json";         //путь к файлу с предсказаниями
        private string[] _predictions;
        private Random _random = new Random();
        public Form1()
        {
            InitializeComponent();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = APP_NAME;

            try
            {
                var data = File.ReadAllText(PREDICTIONS_CONFIG_PATH);

                _predictions = JsonConvert.DeserializeObject<string[]>(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_predictions == null)
                {
                    Close();
                }
                else if (_predictions.Length == 0)
                {
                    MessageBox.Show("Предсказания закончились!");
                    Close();
                }
            }
        }

        //С помощью асинхронности мы ждём пока не выполниться наш Task и только после его выполнения выводим на экран сообщение
        private async void bPredict_Click(object sender, EventArgs e)
        {
            /*
            Для того, чтобы не блокировать интерфейс пользователя (UI) во время выполнения логики нашей программы, нам необходимо всю логику вынести в отдельный поток.
            Но, т.к. в этом отдельном потоке, логика нашей программы обновляет progressBar текущего потока, мы получим исключение. (Недопустимая операция в нескольких
            потоках: попытка доступа к элементу управления 'progressBar1' не из того потока, в котором он был создан)
            Эту ошибку можно избежать с помощью делегатов. (строка 43)
             */
            bPredict.Enabled = false;                                   //блокируем кнопку, чтобы не запускать сразу несколько задач (await-ов)
            await Task.Run(() =>
            {
                for (int i = 1; i <= 100; i++)
                {
                    this.Invoke(new Action(() =>
                    {
                        progressBar1.Value = i;                         //модифицируем данные из другого потока через делегат
                        this.Text = $"{i}%";
                    }));           
                    progressBar1.Value = i;
                    Thread.Sleep(20);
                }
            });

            var index = _random.Next(_predictions.Length);
            var prediction = _predictions[index];

            MessageBox.Show($"{prediction}!");

            progressBar1.Value = 0;                                     //после выполнения сбрасываем в 0
            this.Text = APP_NAME;                                       //снова выводим название приложения
            bPredict.Enabled = true;                                    //снова разблокируем кнопку
        }
    }
}
