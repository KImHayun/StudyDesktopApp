using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace IotSensorMonApp
{
    public partial class FrmMain : Form
    {
        private double xCount = 200; //차트에 보이는 데이터 수
        //변수선언모음
        private Timer timerSimul = new Timer();
        private Random randPhoto = new Random();
        private bool IsSimulation = false;
        private List<SensorData> sensors = new List<SensorData>(); //차트, 리스트박스에 출력
        private string connString = "Data Source=127.0.0.1;Initial Catalog=IoTData;" +
            "Persist Security Info=True;User " +
            "ID=sa;Password=mssql_p@ssw0rd!";
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //ComboBox 초기화
            foreach(var port in SerialPort.GetPortNames())
                {
                CboSerialPort.Items.Add(port);
            }
                CboSerialPort.Text = "Select Port";

            //IOT장비에서 받을 값의 범위
            PrbPhotoResistor.Minimum = 0;
            PrbPhotoResistor.Maximum = 1023;
            PrbPhotoResistor.Value = 0;

            //차트모양 초기화
            InitChartStyle();

            //BtnDisplay 초기화
            BtnDisplay.BackColor = Color.BlueViolet;
            BtnDisplay.ForeColor = Color.WhiteSmoke;
            BtnDisplay.Text = "NONE";
            BtnDisplay.Font = new Font("맑은 고딕", 14, FontStyle.Bold);

            //나머지 초기화
            LblConnectTime.Text = "Connection Time";
            TxtSensorNumber.TextAlign = HorizontalAlignment.Right;
            TxtSensorNumber.Text = "0";
            BtnConnect.Enabled = BtnDisconnect.Enabled = false;
        }

        //차트 초기설정
        private void InitChartStyle()
        {
            ChtPhotoResistor.ChartAreas[0].BackColor = Color.Blue;
            ChtPhotoResistor.ChartAreas[0].AxisX.Minimum = 0;
            ChtPhotoResistor.ChartAreas[0].AxisX.Maximum = xCount; //차트에 보이는 데이터 수
            ChtPhotoResistor.ChartAreas[0].AxisX.Interval = xCount / 4;
            ChtPhotoResistor.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.WhiteSmoke;
            ChtPhotoResistor.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            ChtPhotoResistor.ChartAreas[0].BackColor = Color.Blue;
            ChtPhotoResistor.ChartAreas[0].AxisY.Minimum = 0;
            ChtPhotoResistor.ChartAreas[0].AxisY.Maximum = 1024; //차트에 보이는 데이터 수
            ChtPhotoResistor.ChartAreas[0].AxisY.Interval = 200;
            ChtPhotoResistor.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.WhiteSmoke;
            ChtPhotoResistor.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            ChtPhotoResistor.ChartAreas[0].CursorX.AutoScroll = true;
            ChtPhotoResistor.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            ChtPhotoResistor.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            ChtPhotoResistor.ChartAreas[0].AxisX.ScrollBar.BackColor = Color.LightSteelBlue;

            ChtPhotoResistor.Series.Clear(); //Serial1값 지움
            ChtPhotoResistor.Series.Add("PhotoValue");
            ChtPhotoResistor.Series[0].ChartType = SeriesChartType.Line;
            ChtPhotoResistor.Series[0].Color = Color.LightCoral;
            ChtPhotoResistor.Series[0].BorderWidth = 3;

            //범례삭제
            if (ChtPhotoResistor.Legends.Count > 0)
                for (int i = 0; i < ChtPhotoResistor.Legends.Count; i++)
            {
                    ChtPhotoResistor.Legends.RemoveAt(i);
            }
           

        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            //ToDo 나중에 실제 작업시 작성
        }

       
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            //ToDo 나중에 실제 작업시 작성
        }

        private void MnuBeginSimulation_Click(object sender, EventArgs e) //시뮬레이션 시작
        {
            IsSimulation = true;
            timerSimul.Enabled = true;
            timerSimul.Interval = 1000; //1초
            timerSimul.Tick += TimerSimul_Tick; 
            timerSimul.Start();

        }

        private void TimerSimul_Tick(object sender, EventArgs e)
        {
            int value = randPhoto.Next(2, 1023); // 1부터 1023까지 사이의 값을 받음
            ShowSensorValue(value.ToString());
        }

        private void ShowSensorValue(string value)
        {
            int.TryParse(value, out int v);

            var currentTime = DateTime.Now;
            SensorData data = new SensorData(currentTime, v, IsSimulation);
            sensors.Add(data);
            InsertTable(data);

            //센서값 갯수
            TxtSensorNumber.Text = $"{sensors.Count}";
            //프로그레스바 실시간 현재값 출력
            PrbPhotoResistor.Value = v;
            //리스트박스에 데이터 출력
            var item = $"{currentTime.ToString("yyyy-MM-dd HH:mm:ss") }\t{v}";
            LsbPhotoResistor.Items.Add(item);
            LsbPhotoResistor.SelectedIndex = LsbPhotoResistor.Items.Count - 1; //스크롤 처리


            //차트에 데이터 출력
            ChtPhotoResistor.Series[0].Points.Add(v);

            //200개 넘으면
            ChtPhotoResistor.ChartAreas[0].AxisX.Minimum = 0;
            ChtPhotoResistor.ChartAreas[0].AxisY.Maximum = (sensors.Count >= xCount) ? sensors.Count : xCount;

            //Zoom처리
            if (sensors.Count > xCount)
                ChtPhotoResistor.ChartAreas[0].AxisX.ScaleView.Zoom(sensors.Count - xCount, sensors.Count);
            else
                ChtPhotoResistor.ChartAreas[0].AxisX.ScaleView.Zoom(0, xCount);
            //BtnDisplay 표시
            if (IsSimulation)
                BtnDisplay.Text = v.ToString();
            else
                BtnDisplay.Text = "~" + "\n" + v.ToString();
        }
        //IoT데이터베이스 내 Tbl_PhotoResistor 테이블에 센서데이터 입력

        private void InsertTable(SensorData data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    var query = $"insert into Tbl_PhotoResistor" +
                        $"(CurrentDt, Value, SimulFlag)" +
                        $" values" +
                        $" ('{data.Current.ToString("yyyy-MM-dd HH:mm:ss")}', '{data.Value}', '{(data.SimulFlag == true ? "1" : "0")}');";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"예외발생");
            }
        }

        private void MnuEndSimulation_Click(object sender, EventArgs e) //시뮬레이션 끝
        {
            IsSimulation = false;
            timerSimul.Stop();
        }



        private void MnuExit_Click(object sender, EventArgs e) //종료버튼 클릭
        {
            if (IsSimulation)
            {
                MessageBox.Show("시뮬레이션을 멈춘 후 프로그램을 종료하세요" , "종료",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void BtnViewAll_Click(object sender, EventArgs e)
        {
            ChtPhotoResistor.ChartAreas[0].AxisX.Minimum = 0;
            ChtPhotoResistor.ChartAreas[0].AxisX.Maximum = sensors.Count;

            ChtPhotoResistor.ChartAreas[0].AxisX.ScaleView.Zoom(0, sensors.Count);
            ChtPhotoResistor.ChartAreas[0].AxisX.Interval = sensors.Count / 4;

        }

        private void BtnZoom_Click(object sender, EventArgs e)
        {
            ChtPhotoResistor.ChartAreas[0].AxisX.Minimum = 0;
            ChtPhotoResistor.ChartAreas[0].AxisX.Maximum = sensors.Count;

            ChtPhotoResistor.ChartAreas[0].AxisX.ScaleView.Zoom(0, sensors.Count);
            ChtPhotoResistor.ChartAreas[0].AxisX.Interval = sensors.Count / 4;

        }
    }
}
