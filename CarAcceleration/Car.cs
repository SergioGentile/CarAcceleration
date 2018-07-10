using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;

public class Car
{
    private int speed;
    private DateTime time;
    private SqlConnection connection;
    private Label lblSpeed, lblTime, lblStatus;
    public Car(Label lblSpeed, Label lblTime, Label lblStatus)
	{
   
        speed = 0;
        time = DateTime.Now;
        this.lblSpeed = lblSpeed;
        this.lblTime = lblTime;
        this.lblStatus = lblStatus;
        setCurrentInformation();
        setLabel(lblStatus, "Status: Not Ready. I'm connecting to the service.\nWait...");
        lblStatus.ForeColor = System.Drawing.Color.Red;
        setLabel(lblSpeed, "");
        setLabel(lblTime, "");
        Thread t = new Thread(startLoop);
        t.Start();
    }

    private void startLoop()
    {
        connection = new SqlConnection();
        connection.ConnectionString = "Server=.\\SQL2016; Database = CarSpeed; Integrated Security = True;";
        connection.Open();
        clearDatabase();
        //Start infinite loop that check for update into SQL Server
        setLabel(lblStatus, "Status: Service start");
        lblStatus.ForeColor = System.Drawing.Color.Green;
        setCurrentInformation();
        Thread t = new Thread(fetchFromDatabase);
        t.Start();
    }
  
    private void clearDatabase()
    {
        String query = "DROP TABLE IF EXISTS dbo.T_Speed";
        SqlCommand command = new SqlCommand(query, connection);
        command.ExecuteNonQuery();
        query = "CREATE TABLE dbo.T_Speed (ID INT IDENTITY(1,1) PRIMARY KEY, SPEED INT, TIMES DATETIME)";
        command = new SqlCommand(query, connection);
        command.ExecuteNonQuery();
    }

    private void updateDatabase()
    {
        String query = "INSERT INTO dbo.T_Speed (SPEED, TIMES) VALUES (@speed,@times)";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@speed", speed);
        command.Parameters.AddWithValue("@times", DateTime.Now);
        command.ExecuteNonQuery();
    }

    private void setCurrentInformation()
    {
        if (this.lblSpeed.InvokeRequired)
        {
            this.lblSpeed.BeginInvoke((MethodInvoker)delegate () { this.lblSpeed.Text = "Current speed: " + this.speed + " Km/h";  });
        }
        else
        {
            this.lblSpeed.Text = "Current speed: " + this.speed + " Km/h"; 
        }
    }

    private void setLabel(Label lbl, string text)
    {
        if (lbl.InvokeRequired)
        {
            lbl.BeginInvoke((MethodInvoker)delegate () { lbl.Text = text; });
        }
        else
        {
            lbl.Text = text;
        }
    }

    private void sendToPowerBI()
    {

        String urlPowerBI = "https://api.powerbi.com/beta/b00367e2-193a-4f48-94de-7245d45c0947/datasets/9ac07350-fe0c-42ec-bc82-f94bfd14bf56/rows?key=q0mZX74BEtH7ng6fkgcW5nrGGE8yI3tIyd%2BfAZ6myt0VxdwVRm4QgOrPPOpLllFXYFvcrNiDUE01p3cDEeD5IA%3D%3D";
        String json = "[{\"speed\" : " + speed + ",\"time\" : \"" + time + "\", \"max\" : 100, \"min\": 0}]";
        Debug.WriteLine("POST: " + json);

        HttpClient client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = client.PostAsync(urlPowerBI, content).Result;
       
        Debug.WriteLine("Response code: " + result.StatusCode);
    }

    public void fetchFromDatabase()
    {

        DateTime oldDT = DateTime.Now;
        int lastSpeed = -1;
        while (true)
        {
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM T_Speed WHERE TIMES = (SELECT MAX(TIMES) FROM T_Speed);";
            command.Connection = connection;
            SqlDataReader sReader;
            command.Parameters.Clear();
            sReader = command.ExecuteReader();

            if (sReader.Read())
            {
                if (lastSpeed!= sReader.GetInt32(1))
                {
                    lastSpeed = sReader.GetInt32(1);
                    speed = lastSpeed;
                    time = sReader.GetDateTime(2);
                    setCurrentInformation();
                    sendToPowerBI();
                    
                }
            }
            sReader.Close();
        }
      
    }
    
}
