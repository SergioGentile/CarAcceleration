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
    //URL used by the dashboard of powerBI
    private static readonly String urlPowerBI = "https://api.powerbi.com/beta/b00367e2-193a-4f48-94de-7245d45c0947/datasets/9ac07350-fe0c-42ec-bc82-f94bfd14bf56/rows?key=q0mZX74BEtH7ng6fkgcW5nrGGE8yI3tIyd%2BfAZ6myt0VxdwVRm4QgOrPPOpLllFXYFvcrNiDUE01p3cDEeD5IA%3D%3D";

    public Car(Label lblSpeed, Label lblTime, Label lblStatus)
	{
        
        speed = 0;
        time = DateTime.Now;
        //Take all the references to update the UI
        this.lblSpeed = lblSpeed;
        this.lblTime = lblTime;
        this.lblStatus = lblStatus;

        //Update the UI 
        setCurrentInformation();
        setLabel(lblStatus, "Status: Not Ready. I'm connecting to the service.\nWait...");
        lblStatus.ForeColor = System.Drawing.Color.Red;
        setLabel(lblSpeed, "");
        setLabel(lblTime, "");

        //Start the infinite loop
        Thread t = new Thread(startLoop);
        t.Start();
    }

    private void startLoop()
    {
        //Connect to the database. 
        //I made a connection with the local SQL Server 
        //The name of the database used by the application is CarSpeed.
        try
        {
            connection = new SqlConnection();
            connection.ConnectionString = "Server=.\\SQL2016; Database = CarSpeed; Integrated Security = True;";
            connection.Open(); //Clear and create the table into the database if it doesn't exist.
            clearDatabase();

            //Update the UI
            setLabel(lblStatus, "Status: Service start");
            lblStatus.ForeColor = System.Drawing.Color.Green;
            setCurrentInformation();
            //Start infinite loop that check for update into SQL Server
            Thread t = new Thread(fetchFromDatabase);
            t.Start();
        }
        catch(Exception e)
        {
            Debug.WriteLine(e.Message);
            setLabel(lblStatus, "Status: Service doesn't start properly.");
            lblStatus.ForeColor = System.Drawing.Color.Green;

        }
    }
  
    //Clear the database. Create the table if it doesn't exist.
    private void clearDatabase()
    {
        String query = "DROP TABLE IF EXISTS dbo.T_Speed";
        SqlCommand command = new SqlCommand(query, connection);
        command.ExecuteNonQuery();
        query = "CREATE TABLE dbo.T_Speed (ID INT IDENTITY(1,1) PRIMARY KEY, SPEED INT, TIMES DATETIME)";
        command = new SqlCommand(query, connection);
        command.ExecuteNonQuery();
    }

    
    //Set the current speed and time in the UI.
    private void setCurrentInformation()
    {
        if (this.lblSpeed.InvokeRequired || this.lblTime.InvokeRequired)
        {
            this.lblSpeed.BeginInvoke((MethodInvoker)delegate () { this.lblSpeed.Text = "Current speed: " + this.speed + " Km/h";  });
            this.lblTime.BeginInvoke((MethodInvoker)delegate () { this.lblTime.Text = "Current Time: " + this.time; });
        }
        else
        {
            this.lblSpeed.Text = "Current speed: " + this.speed + " Km/h";
            this.lblTime.Text = "Current Time: " + this.time;
        }
    }

    //Set a text into the specified label 
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


    //This method continuously read information from the database and understand when a new information must be send to the dashboard. 
    private void fetchFromDatabase()
    {

        DateTime oldDT = DateTime.Now;
        int lastSpeed = -1;
        //Create the command to retrieve the last speed inserted into the table
        SqlCommand command = new SqlCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = "SELECT * FROM T_Speed WHERE TIMES = (SELECT MAX(TIMES) FROM T_Speed);";
        command.Connection = connection;
        SqlDataReader sReader;

        //Polling
        while (true)
        {
            sReader = command.ExecuteReader();
            if (sReader.Read())
            {
                //If the speed are different, i must update the dashboard
                if (lastSpeed!= sReader.GetInt32(1))
                {
                    //Retrieve the actual speed and time of the measure
                    lastSpeed = sReader.GetInt32(1);
                    speed = lastSpeed;
                    time = sReader.GetDateTime(2);
                    //Update the UI
                    setCurrentInformation();
                    //Perform an HTTP POST
                    sendToPowerBI(urlPowerBI);
                    
                }
            }
            sReader.Close();
        }
      
    }

    private void sendToPowerBI(String urlPowerBI)
    {
        //Create a json object with the useful information for the POST.
        String json = "[{\"speed\" : " + speed + ",\"time\" : \"" + time + "\", \"max\" : 100, \"min\": 0}]";
        Debug.WriteLine("POST: " + json);

        //Perform the HTTP POST
        HttpClient client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = client.PostAsync(urlPowerBI, content).Result;

        Debug.WriteLine("Response code: " + result.StatusCode);
    }

}
