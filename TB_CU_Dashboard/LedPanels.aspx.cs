using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;
using System.Collections;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
namespace TB_CU_Dashboard
{
    public partial class LedPanels : System.Web.UI.Page
    {
        SqlDataAdapter daCU; // qty pe shift
        //SqlDataAdapter daTB;
        SqlDataAdapter daScrapIV;

        DataSet dsCU = new DataSet();
        //DataSet dsTB = new DataSet();
        DataSet dsScrapIV = new DataSet();

        StringBuilder htmlTable = new StringBuilder();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                BindData();
        }

        private void BindData()
        {///====== Variable declaration =======
            string connectionString;
            SqlConnection cnn;

            string connectionERT;
            SqlConnection cnnERT;

            string connectionMCAT;
            SqlConnection cnnMCAT;
            //======= Set connection string ======
            connectionString = @"Data Source= TITM09S03MS03\TIT_I3; Initial Catalog= ctReports; User ID=ctReports; Password=Rep0rt1ng";//pentru SFI
            connectionERT = @"Data Source= TIAS007A ; Initial Catalog = NewERT; User ID=tableau_user; Password=!Tabl3au";//Pentru ERT - folosit pentru extragere buget
            connectionMCAT = @"Data Source= TITM15C01DB02\TI_MCAT_HIST ; Initial Catalog = HistorianAndReports; User ID=tableauuser; Password=Rep0rt1ng";
            //======= Assign connection
            cnn = new SqlConnection(connectionString);
            cnnERT = new SqlConnection(connectionERT);
            cnnMCAT = new SqlConnection(connectionMCAT);
            //======= Open connection 
            cnn.Open();
            cnnERT.Open();
            cnnMCAT.Open();
            //Label1.Text = "connection succesfull";
            // Quantity CU pe shift:
            SqlCommand cmd = new SqlCommand(@"SELECT TOP 1000 [area]
            ,[Quantity]
            ,[Forecast]
            FROM [NewERT].[dbo].[LedPanels_TB_CU]
            order by area desc
            ", cnnERT);
            SqlCommand cmdScrapIV = new SqlCommand(@"EXEC	[dbo].[sp_ScrapByShift] 
            ", cnnMCAT);

            daCU = new SqlDataAdapter(cmd);
            daCU.Fill(dsCU);
            //daTB = new SqlDataAdapter(cmdTB);
            //daTB.Fill(dsTB);
            daScrapIV = new SqlDataAdapter(cmdScrapIV);
            daScrapIV.Fill(dsScrapIV);

            cmd.ExecuteNonQuery();
            //cmdTB.ExecuteNonQuery();
            cmdScrapIV.ExecuteNonQuery();
            cnn.Close();
            cnnERT.Close();
            cnnMCAT.Close();


            //Declare shift start //  pentru a afla ce schimb lucreaza acum
            TimeSpan Shift1 = TimeSpan.FromHours(7);
            TimeSpan Shift2 = TimeSpan.FromHours(15);
            TimeSpan Shift3 = TimeSpan.FromHours(23);
            TimeSpan Midnight = TimeSpan.FromHours(00);

            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            //-------------------------------------------------
            double ShiftTotalTime = 8 * 60; //480 min pe shift
            double DayTotalTime = 24 * 60; //1440 min intr-o zi
            double minutes; //de la inceputul shift-ului
            double minutesDay; // de la inceputul zilei de lucru
            double min;
            //----------------------------
            DateTime dFromFullDay;
            DateTime dFrom;
            DateTime dTo;
            string sDateFrom;
            string sDateFromFullDay = "07:00:00"; // inceputul zilei de lucru
            string sDateTo = Convert.ToString(currentTime); //Now time
            int h, m, s;
            int hour, mins, secs;
            string dayDiff;
            string timeDiff;
            double intervaleTrecute;
            double Target;
            double DailyTarget;
            //double EfficiencyPrevCU;
            //double EfficiencyPrevTB;
            ////////////// Conditia pe schimburi /////////////////////////////////////////////////////
            htmlTable.Append("<table border='2px solid bold' style='border-collapse: collapse;text-align:center; border-color:white; text-align: center; background-color: black; color: white;'>");
            htmlTable.Append("<tr><font face='arial'><th>Timisoara</th><th>Realizat</th><th>Target</th><th>Target Zilnic</th></font></tr>");
            if (currentTime >= Shift1 && currentTime < Shift2)
            {
                //====================================== SHIFT 1 ================================================================

                //if (!object.Equals(dsCU.Tables[0], null))
                //{
                sDateFrom = "07:00:00"; // shift 1 start time
                if (DateTime.TryParse(sDateFrom, out dFrom) && DateTime.TryParse(sDateTo, out dTo) && DateTime.TryParse(sDateFromFullDay, out dFromFullDay))
                {
                    //Numara cate minute au trecut de la inceputul zilei de lucru:
                    /*TimeSpan TD = dTo - dFromFullDay;
                    h = TD.Hours;
                    m = TD.Minutes;
                    s = TD.Seconds;
                    dayDiff = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
                    minutesDay = TimeSpan.Parse(dayDiff).TotalMinutes;*/

                    //Numara cate minute au trecut de la inceputul shiftului:
                    TimeSpan TS = dTo - dFrom;
                    minutes = TS.TotalMinutes;

                    hour = TS.Hours;
                    mins = TS.Minutes;
                    secs = TS.Seconds;
                    timeDiff = hour.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                    Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 1: " + timeDiff;
                    //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                    //Intervale trecute
                    intervaleTrecute = minutes / 10;
                    int salvat = Convert.ToInt32(intervaleTrecute);
                    Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);


                    if (dsCU.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                        {
                            DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                            Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                            //Label1.Text = Convert.ToString(DailyTarget);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + dsCU.Tables[0].Rows[i]["area"] + " [anvelope] " + "</font></td>");
                            if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                            {
                                htmlTable.Append("<td><font face='arial' color='green'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                            }
                            htmlTable.Append("<td style='color:white;'><b><font face='arial'>" + Convert.ToInt32(Target) + "</font></b></td>");
                            htmlTable.Append("<td><b><font face='arial'>" + dsCU.Tables[0].Rows[i]["Forecast"] + "</font></b></td>");
                            htmlTable.Append("</tr>");
                        }
                    }
                    int TargetScrapDay = 780; // anul 2020. Daily goal (OAT_Daily Trend)
                    double TargetScrap;
                    if (dsScrapIV.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                        {
                            TargetScrap = ((TargetScrapDay / 24) / 6) * Convert.ToInt32(intervaleTrecute);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + " Scrap IV [anvelope] " + "</font></td>");
                            if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                            {
                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='green'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                            }
                            htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + TargetScrapDay + "</font></b></td>");
                            htmlTable.Append("</tr>");
                        }
                    }
                    //=====================Scrolling Text=====================
                    htmlTable.Append("<tr>");
                    htmlTable.Append(@"<td colspan='4' style='background-color:black; color:white;'><b><font face='arial'>
                        <marquee behavior='scroll' direction='left' scrollamount='3'>Continental Anvelope Timisoara    
                        </marquee> </font>
                        </b> </td>");
                    htmlTable.Append("<tr>");

                }
            }
            else //SHIFT 2
            {
                if (currentTime >= Shift2 && currentTime < Shift3)
                {
                    // Shift 2===================================================================================================================
                    sDateFrom = "15:00:00"; // shift 2 start time

                    if (DateTime.TryParse(sDateFrom, out dFrom) && DateTime.TryParse(sDateTo, out dTo) && DateTime.TryParse(sDateFromFullDay, out dFromFullDay))
                    {
                        //Numara cate minute au trecut de la inceputul zilei de lucru:
                        /* TimeSpan TD = dTo - dFromFullDay;
                         h = TD.Hours;
                         m = TD.Minutes;
                         s = TD.Seconds;
                         dayDiff = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
                         minutesDay = TimeSpan.Parse(dayDiff).TotalMinutes;*/

                        //Numara cate minute au trecut de la inceputul shiftului:
                        TimeSpan TS = dTo - dFrom;
                        minutes = TS.TotalMinutes;

                        hour = TS.Hours;
                        mins = TS.Minutes;
                        secs = TS.Seconds;
                        timeDiff = hour.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                        Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 2: " + timeDiff;
                        //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                        //Intervale trecute
                        intervaleTrecute = minutes / 10;
                        int salvat = Convert.ToInt32(intervaleTrecute);
                        Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);


                        if (dsCU.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                            {
                                DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                //Label1.Text = Convert.ToString(DailyTarget);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + dsCU.Tables[0].Rows[i]["area"] + " [anvelope] " + "</font></td>");
                                if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                {
                                    htmlTable.Append("<td><font face='arial' color='green'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                }
                                htmlTable.Append("<td style='color:white;'><b><font face='arial'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                htmlTable.Append("<td><b><font face='arial'>" + dsCU.Tables[0].Rows[i]["Forecast"] + "</font></b></td>");
                                htmlTable.Append("</tr>");
                            }
                        }
                        int TargetScrapDay = 623;
                        double TargetScrap;
                        if (dsScrapIV.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                            {
                                TargetScrap = ((TargetScrapDay / 24) / 6) * Convert.ToInt32(intervaleTrecute);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + " Scrap IV [anvelope] " + "</font></td>");
                                if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                {
                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='green'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                }
                                htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + TargetScrapDay + "</font></b></td>");
                                htmlTable.Append("</tr>");
                            }
                        }
                        //=====================Scrolling Text=====================
                        htmlTable.Append("<tr>");
                        htmlTable.Append(@"<td colspan='4' style='background-color:black; color:white;'><b><font face='arial'>
                        <marquee behavior='scroll' direction='left' scrollamount='3'>Continental Anvelope Timisoara    
                        </marquee> </font>
                        </b> </td>");
                        htmlTable.Append("<tr>");


                    }
                }
                else
                {
                    //Shift 3======================================================================================================

                    if (currentTime >= Shift3 && currentTime <= TimeSpan.Parse("23:59:59"))
                    {
                        sDateFrom = "23:00:00";
                        if (DateTime.TryParse(sDateFrom, out dFrom) && DateTime.TryParse(sDateTo, out dTo))
                        {

                            //Numara cate minute au trecut de la inceputul zilei de lucru:
                            /*TimeSpan TD = dTo - dFromFullDay;
                            h = TD.Hours;
                            m = TD.Minutes;
                            s = TD.Seconds;
                            dayDiff = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00"); /// ERROR !!!!!!!!!!!! 
                            minutesDay = TimeSpan.Parse(dayDiff).TotalMinutes;*/

                            //Numara cate minute au trecut de la inceputul shiftului:
                            TimeSpan TS = dTo - dFrom;
                            minutes = TS.TotalMinutes;

                            hour = TS.Hours;
                            mins = TS.Minutes;
                            secs = TS.Seconds;
                            timeDiff = hour.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                            Label1.Text = "Cat timp a trecut de cand a inceput shift-ul  TEST: " + timeDiff;
                            //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                            //Intervale trecute
                            intervaleTrecute = minutes / 10;
                            int salvat = Convert.ToInt32(intervaleTrecute);
                            Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);


                            if (dsCU.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                                {
                                    DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                    Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                    //Label1.Text = Convert.ToString(DailyTarget);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + dsCU.Tables[0].Rows[i]["area"] + " [anvelope] " + "</font></td>");
                                    if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='green'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                    }
                                    htmlTable.Append("<td style='color:white;'><b><font face='arial'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                    htmlTable.Append("<td><b><font face='arial'>" + dsCU.Tables[0].Rows[i]["Forecast"] + "</font></b></td>");
                                    htmlTable.Append("</tr>");
                                }
                            }
                            int TargetScrapDay = 623;
                            double TargetScrap;
                            if (dsScrapIV.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                                {
                                    TargetScrap = ((TargetScrapDay / 24) / 6) * Convert.ToInt32(intervaleTrecute);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + " Scrap IV [anvelope] " + "</font></td>");
                                    if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='green'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                    }
                                    htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + TargetScrapDay + "</font></b></td>");
                                    htmlTable.Append("</tr>");
                                }
                            }
                            //=====================Scrolling Text=====================
                            htmlTable.Append("<tr>");
                            htmlTable.Append(@"<td colspan='4' style='background-color:black; color:white;'><b><font face='arial'>
                        <marquee behavior='scroll' direction='left' scrollamount='3'>Continental Anvelope Timisoara    
                        </marquee> </font>
                        </b> </td>");
                            htmlTable.Append("<tr>");
                        }

                    }
                    else
                    {
                        if (currentTime >= TimeSpan.Parse("00:00:00") && currentTime < Shift1)
                        {
                            if (DateTime.TryParse("00:00:00", out dFrom) && DateTime.TryParse(sDateTo, out dTo))
                            {

                                //Numara cate minute au trecut de la inceputul zilei de lucru:
                                /*TimeSpan TD = dTo - dFromFullDay;
                                h = TD.Hours;
                                m = TD.Minutes;
                                s = TD.Seconds;
                                dayDiff = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00"); /// ERROR !!!!!!!!!!!! 
                                minutesDay = TimeSpan.Parse(dayDiff).TotalMinutes;*/

                                //Numara cate minute au trecut de la inceputul shiftului:
                                TimeSpan TS = dTo - dFrom;
                                minutes = TS.TotalMinutes;
                                min = minutes + 60;

                                hour = TS.Hours + 1;
                                mins = TS.Minutes;
                                secs = TS.Seconds;
                                timeDiff = hour.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");
                                Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 3: " + timeDiff;
                                //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                                //Intervale trecute
                                intervaleTrecute = (min / 10) + 6;  // se aduna o ora (6 intervale) din ziua anterioara
                                int salvat = Convert.ToInt32(intervaleTrecute);
                                Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);

                                if (dsCU.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                                    {
                                        DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                        Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                        //Label1.Text = Convert.ToString(DailyTarget);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + dsCU.Tables[0].Rows[i]["area"] + " [anvelope] " + "</font></td>");
                                        if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='green'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                        }
                                        htmlTable.Append("<td style='color:white;'><b><font face='arial'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                        htmlTable.Append("<td><b><font face='arial'>" + dsCU.Tables[0].Rows[i]["Forecast"] + "</font></b></td>");
                                        htmlTable.Append("</tr>");
                                    }
                                }
                                int TargetScrapDay = 623;
                                double TargetScrap;
                                if (dsScrapIV.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                                    {
                                        TargetScrap = ((TargetScrapDay / 24) / 6) * Convert.ToInt32(intervaleTrecute);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + " Scrap IV [anvelope] " + "</font></td>");
                                        if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='green'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                        }
                                        htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + TargetScrapDay + "</font></b></td>");
                                        htmlTable.Append("</tr>");
                                    }
                                }
                                //=====================Scrolling Text=====================
                                htmlTable.Append("<tr>");
                                htmlTable.Append(@"<td colspan='4' style='background-color:black; color:white;'><b><font face='arial'>
                        <marquee behavior='scroll' direction='left' scrollamount='3'>Continental Anvelope Timisoara    
                        </marquee> </font>
                        </b> </td>");
                                htmlTable.Append("<tr>");

                            }
                        }
                    }
                }
            }

            htmlTable.Append("</tr>");
            htmlTable.Append("</table>");
            DBDataPlaceHolder.Controls.Add(new Literal { Text = htmlTable.ToString() });
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }
    }
}