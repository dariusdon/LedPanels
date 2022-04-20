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
    public partial class LedPanelsDashboard : System.Web.UI.Page
    {
        SqlDataAdapter daCU; // qty pe shift
        //SqlDataAdapter daTB;
        SqlDataAdapter daScrapIV;
        SqlDataAdapter daLD;
        SqlDataAdapter daFCY;
        SqlDataAdapter daSA;
        SqlDataAdapter daScrapLastDay;
        SqlDataAdapter daShiftID;
        SqlDataAdapter daScrapLastShift;

        DataSet dsCU = new DataSet();
        //DataSet dsTB = new DataSet();
        DataSet dsScrapIV = new DataSet();
        DataSet dsLD = new DataSet();
        DataSet dsFCY = new DataSet();
        DataSet dsSA = new DataSet();
        DataSet dsScrapLastDay = new DataSet();
        DataSet dsShiftID = new DataSet();
        DataSet dsScrapLastShift = new DataSet();

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
            connectionERT = @"Data Source= TIAS067A ; Initial Catalog = NewERT; User ID=sa; Password=Server1nst";//Pentru ERT - folosit pentru extragere buget
            connectionMCAT = @"Data Source= TITM15C02DB02\TI_MCAT_HIST ; Initial Catalog = HistorianAndReports; User ID=tableauuser; Password=Rep0rt1ng";
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
            SqlCommand cmd = new SqlCommand(@" SELECT TOP 1000 [area]
,[Quantity]
+ ( select isnull(sum(QuantityOK),0) as [Quantity]
FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
on s.WorkCenterID=w.WorkCenterID
where 
convert(varchar,s.ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '23:59:59' then convert(varchar, getdate(), 23)  
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate()-1, 23)
end
and s.ShiftNumber = case when convert(varchar, getdate(), 8) between '07:00:00' and '15:00:00' then 1
when convert(varchar, getdate(), 8) between '15:00:00' and '23:00:00' then 2
when convert(varchar, getdate(), 8) between '23:00:00' and '06:59:59' then 3
end
and w.Name like '%CU%') as [Quantity]
,[Forecast]
FROM [NewERT].[dbo].[LedPanels_TB_CU]
where area like 'CU'


union
SELECT TOP 1000 [area]
,[Quantity]
+ ( select isnull(sum(QuantityOK),0) as [Quantity]
FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
on s.WorkCenterID=w.WorkCenterID
where 
convert(varchar,s.ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '23:59:59' then convert(varchar, getdate(), 23)  
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate()-1, 23)
end
and s.ShiftNumber = case when convert(varchar, getdate(), 8) between '07:00:00' and '15:00:00' then 1
when convert(varchar, getdate(), 8) between '15:00:00' and '23:00:00' then 2
when convert(varchar, getdate(), 8) between '23:00:00' and '06:59:59' then 3
end
and w.Name like '%TBM%') as [Quantity]
,[Forecast]
FROM [NewERT].[dbo].[LedPanels_TB_CU]
where area like 'TB'
order by area desc

            ", cnnERT);
            SqlCommand cmdScrapIV = new SqlCommand(@"EXEC	[dbo].[sp_ScrapByShift] 
            ", cnnMCAT);
            SqlCommand cmdForecastyesterday = new SqlCommand(@"SELECT TOP (1000) [ShiftDate]
             ,[Forcast]
             ,[Holiday]
              FROM [NewERT].[dbo].[Forcast_Production_TB_CU]
              where convert(varchar, ShiftDate, 23) = convert(varchar, getdate()-1, 23)
             ", cnnERT);

            SqlCommand cmdShiftAnterior = new SqlCommand(@"Select *
            from
            (Select area='CU'
            ,shift_anterior =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (  select top 1 shift_date
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end) 
            and area like 'CU' and shift_id=(  select top 1 shift_id
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end)) + 
                                                      --- CGMS data
                                                      (      select isnull(sum(QuantityOK),0) as shift_anterior
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '15:00:00' then convert(varchar, getdate()-1, 23)  
 when convert(varchar, getdate(), 8) between '15:00:00' and '23:00:00' then convert(varchar, getdate(), 23)
 when convert(varchar, getdate(), 8) between '23:00:00' and '23:59:59' then convert(varchar, getdate(), 23)   
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate()-1, 23)
end
   and w.Name like '%CU%'
   and s.ShiftNumber = case when convert(varchar, getdate(), 114) between '07:00:00:000' and '15:00:00:000' then 3
                                               when convert(varchar, getdate(), 114) between '15:00:00:000' and '23:00:00:000' then 1
                                               when convert(varchar, getdate(), 114) between '23:00:00:000' and '06:59:59:999' then 2
                                        end)
                                                                                              +
             (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (  select top 1 shift_date
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end) 
            and area like 'CU2' and shift_id=(  select top 1 shift_id
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end))
                                              ) a
            union
            (
            Select area='TB'
            ,shift_anterior =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (  select top 1 shift_date
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end) 
            and area like 'TB' and shift_id=(  select top 1 shift_id
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end)) + 

                                               --- date CGMS
                                               (  select isnull(sum(QuantityOK),0) as shift_anterior
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '15:00:00' then convert(varchar, getdate()-1, 23)  
 when convert(varchar, getdate(), 8) between '15:00:00' and '23:00:00' then convert(varchar, getdate(), 23)
 when convert(varchar, getdate(), 8) between '23:00:00' and '23:59:59' then convert(varchar, getdate(), 23)   
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate()-1, 23)
end
   and w.Name like '%TBM%'
     and s.ShiftNumber = case when convert(varchar, getdate(), 114) between '07:00:00:000' and '15:00:00:000' then 3
                                               when convert(varchar, getdate(), 114) between '15:00:00:000' and '23:00:00:000' then 1
                                               when convert(varchar, getdate(), 114) between '23:00:00:000' and '06:59:59:999' then 2
                                        end) +
             (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (  select top 1 shift_date
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end) 
            and area like 'TB2' and shift_id=(  select top 1 shift_id
              FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
              where shift_id = case when  (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =1  then  3
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =2  then  1 
                                              else case when (select top 1 shift_id FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) =3  then  2
                                              end end end))
                                              )
            order by area desc
            ", cnnERT);

            SqlCommand cmdLastDay = new SqlCommand(@"
select * from
            (
            Select area='TB'
            ,shift_1 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'TB' and shift_id='1') + 
			-- valori CGMS
			(select isnull(sum(QuantityOK),0) as quantity
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%TBM%'
   and s.ShiftNumber=1) + 
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'TB2' and shift_id='1')

            ,shift_2 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'TB' and shift_id='2') + 
			-- Valori CGMS
			(    select isnull(sum(QuantityOK),0) as quantity
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%TBM%'
   and s.ShiftNumber=2) +
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'TB2' and shift_id='2')

            ,shift_3 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'TB' and shift_id='3') + 
			-- valori CGMS
			(  select isnull(sum(QuantityOK),0) as quantity
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '23:59:59' then convert(varchar, getdate()-1, 23)  
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate()-2, 23)
end
   and w.Name like '%TBM%'
   and s.ShiftNumber like '3')+
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'TB2' and shift_id='3')

            ,total = (select sum(quantity)
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext])
            and area like 'TB%')+
			-- valori CGMS
			   (select isnull(sum(QuantityOK),0) 
   FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where  convert(varchar,s.ShiftDate,23) = case when convert(varchar, getdate(), 8) between '07:00:00' and '23:59:59' then convert(varchar, getdate()-1, 23)  
when convert(varchar, getdate(), 8) between '00:00:00' and '06:59:59' then convert(varchar, getdate(), 23)
end
   and w.Name like '%TBM%')
			 ) a

            union (

            Select area='CU'
            ,shift_1 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'CU' and shift_id='1') + 
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'CU2' and shift_id='1')+
			-- valori CGMS
			(   select isnull(sum(QuantityOK),0) as quantity
  FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%CU%'
    and s.ShiftNumber=1)
            ,shift_2 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'CU' and shift_id='2') + 
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'CU2' and shift_id='2')+
			-- valori CGMS
			(	  select isnull(sum(QuantityOK),0) as quantity
  FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%CU%'
    and s.ShiftNumber=2)
            ,shift_3 =  (select quantity
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]) 
            and area like 'CU' and shift_id='3') + 
             (select quantity 
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext] )
            and area like 'CU2' and shift_id='3')+
			-- valori CGMS
			(	  select isnull(sum(QuantityOK),0) as quantity
  FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%CU%'
    and s.ShiftNumber=3)
            , total = ((select sum(quantity)
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext]
            where shift_date = (select top 1 shift_date = convert(varchar,getdate()-1,23)+' 00:00:00.000'
            FROM [TITM09S03MS03\TIT_I3].[ctReports].[dbo].[DSB_Data_ext])
            and area like 'CU%')
			+
			-- valori CGMS
			(	  select isnull(sum(QuantityOK),0) 
  FROM [TITM09CGMDB01].[CGMS].[dbo].[CounterPerShift] s inner join [TITM09CGMDB01].[CGMS].[dbo].[WorkCenter] w
   on s.WorkCenterID=w.WorkCenterID
   where convert(varchar,ShiftDate,23) = convert(varchar, Getdate()-1,23)
   and w.Name like '%CU%')
			)
            ) 
			
            order by area desc
            ", cnnERT);

            SqlCommand cmdLastDayScrap = new SqlCommand(@"select 'Scrap' as value, 
            s1=(select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate()-1, 23)+' 07:00:00.000' and convert(varchar, getdate()-1, 23)+' 15:00:00.000' 
            and Overallgrade like 'X'),
            s2= (select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate()-1, 23)+' 15:00:00.000' and convert(varchar, getdate()-1, 23)+' 23:00:00.000' 
            and Overallgrade like 'X'),
            s3=(select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate()-1, 23)+' 23:00:00.000' and convert(varchar, getdate(), 23)+' 06:59:59.999' 
            and Overallgrade like 'X'),
            total=(select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate()-1, 23)+' 07:00:00.000' and convert(varchar, getdate(), 23)+' 06:59:59.999' 
            and Overallgrade like 'X')", cnnMCAT);

            SqlCommand cmdShiftID = new SqlCommand(@"select shift_id = case when convert(varchar, getdate(), 24)  between '07:00:00' and '15:00:00' then 1 
            else case when convert(varchar, getdate(), 24)  between '15:00:00' and '23:00:00' then 2
            else case when convert(varchar, getdate(), 24)  between '23:00:00' and '07:00:00' then 3
            end end end", cnnERT);

            SqlCommand cmdScrapLastShift = new SqlCommand(@"select 'Scrap' as value, 
            s1=(select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate(), 23)+' 07:00:00.000' and convert(varchar, getdate(), 23)+' 15:00:00.000' 
            and Overallgrade like 'X'),
            s2= (select count(barcode) as scrap
            FROM [TITM15C02DB02\TI_MCAT_HIST].[HistorianAndReports].[dbo].[mcat_GradingDetailData]
            where shiftDateTime between convert(varchar, getdate(), 23)+' 15:00:00.000' and convert(varchar, getdate(), 23)+' 23:00:00.000' 
            and Overallgrade like 'X')", cnnMCAT);
            daCU = new SqlDataAdapter(cmd);
            daCU.Fill(dsCU);
            //daTB = new SqlDataAdapter(cmdTB);
            //daTB.Fill(dsTB);
            daScrapIV = new SqlDataAdapter(cmdScrapIV);
            daScrapIV.Fill(dsScrapIV);
            daLD = new SqlDataAdapter(cmdLastDay);
            daLD.Fill(dsLD);
            daFCY = new SqlDataAdapter(cmdForecastyesterday);
            daFCY.Fill(dsFCY);
            daSA = new SqlDataAdapter(cmdShiftAnterior);
            daSA.Fill(dsSA);
            daScrapLastDay = new SqlDataAdapter(cmdLastDayScrap);
            daScrapLastDay.Fill(dsScrapLastDay);
            daShiftID = new SqlDataAdapter(cmdShiftID);
            daShiftID.Fill(dsShiftID);
            daScrapLastShift = new SqlDataAdapter(cmdScrapLastShift);
            daScrapLastShift.Fill(dsScrapLastShift);

            cmd.ExecuteNonQuery();
            //cmdTB.ExecuteNonQuery();
            cmdScrapIV.ExecuteNonQuery();
            cmdLastDayScrap.ExecuteNonQuery();
            cnn.Close();
            cnnERT.Close();
            cnnMCAT.Close();


            //Declare shift start //  pentru a afla ce schimb lucreaza acum
            TimeSpan Shift1 = TimeSpan.FromHours(7);
            TimeSpan Shift2 = TimeSpan.FromHours(15);
            TimeSpan Shift3 = TimeSpan.FromHours(23);
            TimeSpan Midnight = TimeSpan.FromHours(00);

            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            string currentDay = DateTime.Now.ToString("dd-MM-yyyy");
            string yesterday = DateTime.Today.AddDays(-1).ToString("dd-MM-yyyy");
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
            double RunRateShift;
            double RunRateScrap;
            double intervaleTrecute;
            double Target;
            double DailyTarget;
            double ForecastShift;
            double ForecastShiftY;

            
            int TargetScrapTotal = 870; //780; //720; //900; //780; // Serban a estimat 1050 pe zi
            int TargetScrapShift = 290; // TargetScrapTotal / 3;

            //double EfficiencyPrevCU;
            //double EfficiencyPrevTB;
            ////////////// Conditia pe schimburi /////////////////////////////////////////////////////
            htmlTable.Append("<table border='2px solid bold' style='border-collapse: collapse; width:100%; text-align:center; border-color:white; text-align: center; background-color: black; color: white;'><font size='80'>");
            //htmlTable.Append(@"<tr><td colspan='4'  style='text-align:left; border-right:0px;'> Ziua in curs : </td>
            //                       <td colspan='2'  style='text-align:right; border-left:0px;'>" + currentDay + @"</td>    
            //                 </tr>");
            htmlTable.Append(@"<tr  style='border-top:2px;' ><td><font size='80'>" + currentDay + @"</font></td>
                                   <td><font size='80'> Realizat </font></td>  
                                   <td><font size='80'> Obiectiv </font></td>  
                                   <td><font size='80'> Predicție </font></td>  
                                   <td><font size='80'> Obiectiv Schimb </font></td>  
                                   <td><font size='80'> Schimb Anterior </font></td>  
                             </tr>");
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
                    //Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 1: " + timeDiff;
                    //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                    //Intervale trecute
                    intervaleTrecute = minutes / 10;
                    int salvat = Convert.ToInt32(intervaleTrecute);
                    //Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);

                    double TargetScrap;
                    if (dsScrapIV.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                        {
                            RunRateScrap = Convert.ToDouble(dsScrapIV.Tables[0].Rows[i]["Scrap"]) / minutes * ShiftTotalTime;
                            TargetScrap = ((TargetScrapShift / 8) / 6) * Convert.ToInt32(intervaleTrecute);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + " Scrap IV " + "</font></td>");
                            if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                            {
                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                            }
                            htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                            if (Convert.ToInt32(RunRateScrap) <= TargetScrapShift)
                            {
                                htmlTable.Append("<td style='background-color:black;'><b><font face='arial' color='#64FE2E' size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td style='background-color:black;'><b><font face='arial' color='red' size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                            }
                            htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + TargetScrapShift + "</font></b></td>");
                            if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "1")
                            {// daca e schimbul 1 : se afiseaza valoarea de la schimbul 3 de ieri 
                                if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[0]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                }
                            }
                            else
                            {
                                if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "2")
                                {// daca e schimbul 2 : se afiseaza valoarea de la schimbul 1
                                    if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s1"]) >= TargetScrap)
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                    }
                                }
                                else
                                {// daca e schimbul 3 : se afiseaza valoarea de la schimbul 2 
                                    if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s2"]) >= TargetScrap) //conditia de culoare
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                    }
                                }

                            }
                            htmlTable.Append("</tr>");
                        }
                    }

                    if (dsCU.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                        {
                            ForecastShift = Convert.ToDouble(dsCU.Tables[0].Rows[0]["Forecast"]) / 3;
                            RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                            DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                            Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                            //Label1.Text = Convert.ToString(DailyTarget);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsCU.Tables[0].Rows[i]["area"] + "</font></td>");
                            if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                            }

                            htmlTable.Append("<td style='color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(Target) + "</font></b></td>");
                            if (Convert.ToInt32(RunRateShift) >= Convert.ToInt32(ForecastShift))
                            {
                                htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><b><font face='arial' color='red' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                            }
                            htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(ForecastShift) + "</font></b></td>");
                            if (Convert.ToInt32(dsSA.Tables[0].Rows[i]["shift_anterior"]) >= Convert.ToInt32(ForecastShift))
                            {

                                htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                            }
                            else
                            {

                                htmlTable.Append("<td><b><font face='arial' color='red' size='80'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                            }
                            htmlTable.Append("</tr>");
                        }

                    }
                    //   htmlTable.Append(@"<tr><td colspan='4' style='text-align:left; border-right:0px;'> Ziua anterioara : </td>
                    //      <td colspan='2' style='text-align:right; border-left:0px;'>" + yesterday + @"</td>    
                    //</tr>");
                    htmlTable.Append(@"<tr style='border-top:2px; border-top-style: solid;'><td><font  size='80'>" + yesterday + @" </font> </td>
                                   <td><font  size='80'> S1 </font></td>  
                                   <td><font  size='80'> S2 </font></td>  
                                   <td><font  size='80'> S3 </font></td>  
                                   <td><font  size='80'> Total </font></td>  
                                   <td><font  size='80'> Obiectiv </font></td>  
                             </tr>");
                    if (dsScrapLastDay.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsScrapLastDay.Tables[0].Rows.Count; i++)
                        {
                            ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                            //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                            //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                            //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                            //Label1.Text = Convert.ToString(DailyTarget);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + "Scrap IV" + "</font></td>");
                            if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s1"]) >= Convert.ToInt32(TargetScrapShift))
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                            }
                            if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s2"]) >= Convert.ToInt32(TargetScrapShift))
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                            }
                            if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                            }
                            if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(TargetScrapTotal))
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                            }
                            htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrapTotal) + "</font></b></td>");
                            htmlTable.Append("</tr>");
                        }
                    }
                    if (dsLD.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsLD.Tables[0].Rows.Count; i++)
                        {
                            ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                            //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                            //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                            //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                            //Label1.Text = Convert.ToString(DailyTarget);
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsLD.Tables[0].Rows[i]["area"] + "</font></td>");
                            if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_1"]) >= Convert.ToInt32(ForecastShiftY))
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                            }
                            if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_2"]) >= Convert.ToInt32(ForecastShiftY))
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                            }
                            if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_3"]) >= Convert.ToInt32(ForecastShiftY))
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                            };
                            if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(dsFCY.Tables[0].Rows[0]["Forcast"]))
                            {
                                htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                            }
                            else
                            {
                                htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                            }
                            htmlTable.Append("<td><b><font face='arial' size='80'>" + dsFCY.Tables[0].Rows[0]["Forcast"] + "</font></b></td>");
                            htmlTable.Append("</tr>");
                        }
                    }

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
                        //Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 2: " + timeDiff;
                        //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                        //Intervale trecute
                        intervaleTrecute = minutes / 10;
                        int salvat = Convert.ToInt32(intervaleTrecute);
                        //Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);

                        //int TargetScrapDay = 780 / 3;
                        double TargetScrap;
                        if (dsScrapIV.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                            {
                                RunRateScrap = Convert.ToDouble(dsScrapIV.Tables[0].Rows[i]["Scrap"]) / minutes * ShiftTotalTime;
                                TargetScrap = ((TargetScrapShift / 8) / 6) * Convert.ToInt32(intervaleTrecute);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + " Scrap IV " + "</font></td>");
                                if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                {
                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                }
                                htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                if (Convert.ToInt32(RunRateScrap) <= TargetScrapShift)
                                {
                                    htmlTable.Append("<td style='background-color:black; color:#64FE2E;'><b><font face='arial' size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td style='background-color:black; color:red;'><b><font face='arial'size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                }
                                htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + TargetScrapShift + "</font></b></td>");
                                if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "1")
                                {// daca e schimbul 1 : se afiseaza valoarea de la schimbul 3 de ieri 
                                    if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[0]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                    }
                                }
                                else
                                {
                                    if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "2")
                                    {// daca e schimbul 2 : se afiseaza valoarea de la schimbul 1
                                        if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s1"]) >= TargetScrap)
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                        }
                                    }
                                    else
                                    {// daca e schimbul 3 : se afiseaza valoarea de la schimbul 2 
                                        if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s2"]) >= TargetScrap) //conditia de culoare
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                        }
                                    }

                                }
                                htmlTable.Append("</tr>");
                            }
                        }
                        if (dsCU.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                            {
                                ForecastShift = Convert.ToDouble(dsCU.Tables[0].Rows[0]["Forecast"]) / 3;
                                RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                //Label1.Text = Convert.ToString(DailyTarget);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsCU.Tables[0].Rows[i]["area"] + "</font></td>");
                                if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                }

                                htmlTable.Append("<td style='color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                if (Convert.ToInt32(RunRateShift) >= Convert.ToInt32(ForecastShift))
                                {
                                    htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><b><font face='arial' color='red' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                }
                                htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(ForecastShift) + "</font></b></td>");
                                if (Convert.ToInt32(dsSA.Tables[0].Rows[i]["shift_anterior"]) >= Convert.ToInt32(ForecastShift))
                                {

                                    htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                }
                                else
                                {

                                    htmlTable.Append("<td><b><font face='arial' color='red' size='80'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                }
                                htmlTable.Append("</tr>");
                            }

                        }
                        //   htmlTable.Append(@"<tr><td colspan='4' style='text-align:left; border-right:0px;'> Ziua anterioara : </td>
                        //      <td colspan='2' style='text-align:right; border-left:0px;'>" + yesterday + @"</td>    
                        //</tr>");
                        htmlTable.Append(@"<tr style='border-top:2px; border-top-style: solid;'><td><font size='80'>" + yesterday + @" </font> </td>
                                   <td><font size='80'> S1 </font></td>  
                                   <td><font size='80'> S2 </font></td>  
                                   <td><font size='80'> S3 </font></td>  
                                   <td><font size='80'> Total </font></td>  
                                   <td><font size='80'> Obiectiv </font></td>  
                             </tr>");
                        if (dsScrapLastDay.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsScrapLastDay.Tables[0].Rows.Count; i++)
                            {
                                ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                //Label1.Text = Convert.ToString(DailyTarget);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + "Scrap IV" + "</font></td>");
                                if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s1"]) >= Convert.ToInt32(TargetScrapShift))
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                }
                                if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s2"]) >= Convert.ToInt32(TargetScrapShift))
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                }
                                if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                };
                                if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(TargetScrapTotal))
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                }
                                htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrapTotal) + "</font></b></td>");
                                htmlTable.Append("</tr>");
                            }
                        }
                        if (dsLD.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsLD.Tables[0].Rows.Count; i++)
                            {
                                ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                //Label1.Text = Convert.ToString(DailyTarget);
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsLD.Tables[0].Rows[i]["area"] + "</font></td>");
                                if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_1"]) >= Convert.ToInt32(ForecastShiftY))
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                }
                                if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_2"]) >= Convert.ToInt32(ForecastShiftY))
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                }
                                if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_3"]) >= Convert.ToInt32(ForecastShiftY))
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                };
                                if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(dsFCY.Tables[0].Rows[0]["Forcast"]))
                                {
                                    htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                }
                                else
                                {
                                    htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                }
                                htmlTable.Append("<td><b><font face='arial' size='80'>" + dsFCY.Tables[0].Rows[0]["Forcast"] + "</font></b></td>");
                                htmlTable.Append("</tr>");
                            }
                        }



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
                            //Label1.Text = "Cat timp a trecut de cand a inceput shift-ul  TEST: " + timeDiff;
                            //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                            //Intervale trecute
                            intervaleTrecute = minutes / 10;
                            int salvat = Convert.ToInt32(intervaleTrecute);
                            //Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);

                            //int TargetScrapDay = 780 / 3;
                            double TargetScrap;
                            if (dsScrapIV.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                                {
                                    RunRateScrap = Convert.ToDouble(dsScrapIV.Tables[0].Rows[i]["Scrap"]) / minutes * ShiftTotalTime;
                                    TargetScrap = ((TargetScrapShift / 8) / 6) * Convert.ToInt32(intervaleTrecute);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + " Scrap IV " + "</font></td>");
                                    if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                    }
                                    htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                    if (Convert.ToInt32(RunRateScrap) <= TargetScrapShift)
                                    {
                                        htmlTable.Append("<td style='background-color:black; color:#64FE2E;'><b><font face='arial' size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td style='background-color:black; color:red;'><b><font face='arial' size='80'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                    }
                                    htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial' size='80'>" + TargetScrapShift + "</font></b></td>");
                                    if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "1")
                                    {// daca e schimbul 1 : se afiseaza valoarea de la schimbul 3 de ieri 
                                        if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[0]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "2")
                                        {// daca e schimbul 2 : se afiseaza valoarea de la schimbul 1
                                            if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s1"]) >= TargetScrap)
                                            {
                                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                            }
                                            else
                                            {
                                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                            }
                                        }
                                        else
                                        {// daca e schimbul 3 : se afiseaza valoarea de la schimbul 2 
                                            if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s2"]) >= TargetScrap) //conditia de culoare
                                            {
                                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                            }
                                            else
                                            {
                                                htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                            }
                                        }

                                    }
                                    htmlTable.Append("</tr>");
                                }
                            }
                            if (dsCU.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                                {
                                    ForecastShift = Convert.ToDouble(dsCU.Tables[0].Rows[0]["Forecast"]) / 3;
                                    RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                    DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                    Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                    //Label1.Text = Convert.ToString(DailyTarget);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsCU.Tables[0].Rows[i]["area"] + "</font></td>");
                                    if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                    }

                                    htmlTable.Append("<td style='color:white;'><b><font face='arial' size='80'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                    if (Convert.ToInt32(RunRateShift) >= Convert.ToInt32(ForecastShift))
                                    {
                                        htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><b><font face='arial' color='red' size='80'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                    }
                                    htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(ForecastShift) + "</font></b></td>");
                                    if (Convert.ToInt32(dsSA.Tables[0].Rows[i]["shift_anterior"]) >= Convert.ToInt32(ForecastShift))
                                    {

                                        htmlTable.Append("<td><b><font face='arial' color='#64FE2E' size='80'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                    }
                                    else
                                    {

                                        htmlTable.Append("<td><b><font face='arial' color='red'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                    }
                                    htmlTable.Append("</tr>");
                                }
                            }
                            //   htmlTable.Append(@"<tr><td colspan='4' style='text-align:left; border-right:0px;'> Ziua anterioara : </td>
                            //      <td colspan='2' style='text-align:right; border-left:0px;'>" + yesterday + @"</td>    
                            //</tr>");
                            htmlTable.Append(@"<tr style='border-top:2px;  border-top-style: solid;'><td><font size='80'>" + yesterday + @" </font></td>
                                   <td><font size='80'> S1 </font></td>  
                                   <td><font size='80'> S2 </font></td>  
                                   <td><font size='80'> S3 </font></td>  
                                   <td><font size='80'> Total </font></td>  
                                   <td><font size='80'> Obiectiv </font></td>  
                             </tr>");
                            if (dsScrapLastDay.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsScrapLastDay.Tables[0].Rows.Count; i++)
                                {
                                    ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                    //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                    //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                    //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                    //Label1.Text = Convert.ToString(DailyTarget);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'>" + "Scrap IV" + "</font></td>");
                                    if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s1"]) >= Convert.ToInt32(TargetScrapShift))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                    }
                                    if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s2"]) >= Convert.ToInt32(TargetScrapShift))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                    }
                                    if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                    };
                                    if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(TargetScrapTotal))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E' size='80'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                    }
                                    htmlTable.Append("<td><b><font face='arial' size='80'>" + Convert.ToInt32(TargetScrapTotal) + "</font></b></td>");
                                    htmlTable.Append("</tr>");
                                }
                            }
                            if (dsLD.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsLD.Tables[0].Rows.Count; i++)
                                {
                                    ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                    //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                    //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                    //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                    //Label1.Text = Convert.ToString(DailyTarget);
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial' size='80'> Volum " + dsLD.Tables[0].Rows[i]["area"] + "</font></td>");
                                    if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_1"]) >= Convert.ToInt32(ForecastShiftY))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                    }
                                    if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_2"]) >= Convert.ToInt32(ForecastShiftY))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                    }
                                    if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_3"]) >= Convert.ToInt32(ForecastShiftY))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                    };
                                    if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(dsFCY.Tables[0].Rows[0]["Forcast"]))
                                    {
                                        htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                    }
                                    htmlTable.Append("<td><b><font face='arial'>" + dsFCY.Tables[0].Rows[0]["Forcast"] + "</font></b></td>");
                                    htmlTable.Append("</tr>");
                                }

                            }

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
                                //Label1.Text = "Cat timp a trecut de cand a inceput shift-ul 3: " + timeDiff;
                                //Label3.Text = "Total minute: " + minutes.ToString(); //diferenta in minute

                                //Intervale trecute
                                intervaleTrecute = (min / 10) + 6;  // se aduna o ora (6 intervale) din ziua anterioara
                                int salvat = Convert.ToInt32(intervaleTrecute);
                                //Label3.Text = "Intervale de 10 minute trecute de la inceputul shift-ului: " + Convert.ToString(salvat);
                                //int TargetScrapDay = 780 / 3;
                                double TargetScrap;
                                if (dsScrapIV.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsScrapIV.Tables[0].Rows.Count; i++)
                                    {
                                        RunRateScrap = Convert.ToDouble(dsScrapIV.Tables[0].Rows[i]["Scrap"]) / minutes * ShiftTotalTime;
                                        TargetScrap = ((TargetScrapShift / 8) / 6) * Convert.ToInt32(intervaleTrecute);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + " Scrap IV " + "</font></td>");
                                        if (Convert.ToInt32(dsScrapIV.Tables[0].Rows[i]["Scrap"]) >= TargetScrap)
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E'><b>" + dsScrapIV.Tables[0].Rows[i]["Scrap"] + "</b></font></td>");
                                        }
                                        htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + Convert.ToInt32(TargetScrap) + "</font></b></td>");
                                        if (Convert.ToInt32(RunRateScrap) <= TargetScrapShift)
                                        {
                                            htmlTable.Append("<td style='background-color:black; color:#64FE2E;'><b><font face='arial'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td style='background-color:black; color:red;'><b><font face='arial'>" + Convert.ToInt32(RunRateScrap) + "</font></b></td>");
                                        }
                                        htmlTable.Append("<td style='background-color:black; color:white;'><b><font face='arial'>" + TargetScrapShift + "</font></b></td>");
                                        if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "1")
                                        {// daca e schimbul 1 : se afiseaza valoarea de la schimbul 3 de ieri 
                                            if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[0]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                            {
                                                htmlTable.Append("<td><font face='arial' color='red'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                            }
                                            else
                                            {
                                                htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsScrapLastDay.Tables[0].Rows[0]["s3"] + "</b></font></td>");
                                            }
                                        }
                                        else
                                        {
                                            if (Convert.ToString(dsShiftID.Tables[0].Rows[0]["shift_id"]) == "2")
                                            {// daca e schimbul 2 : se afiseaza valoarea de la schimbul 1
                                                if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s1"]) >= TargetScrap)
                                                {
                                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                                }
                                                else
                                                {
                                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s1"] + "</font></b></td>");
                                                }
                                            }
                                            else
                                            {// daca e schimbul 3 : se afiseaza valoarea de la schimbul 2 
                                                if (Convert.ToInt32(dsScrapLastShift.Tables[0].Rows[0]["s2"]) >= TargetScrap) //conditia de culoare
                                                {
                                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='red'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                                }
                                                else
                                                {
                                                    htmlTable.Append("<td style='background-color:black;'><font face='arial' color='#64FE2E'><b>" + dsScrapLastShift.Tables[0].Rows[0]["s2"] + "</font></b></td>");
                                                }
                                            }

                                        }
                                        htmlTable.Append("</tr>");
                                    }
                                }
                                if (dsCU.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsCU.Tables[0].Rows.Count; i++)
                                    {
                                        ForecastShift = Convert.ToDouble(dsCU.Tables[0].Rows[0]["Forecast"]) / 3;
                                        RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                        DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                        Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                        //Label1.Text = Convert.ToString(DailyTarget);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'> Volum " + dsCU.Tables[0].Rows[i]["area"] + "</font></td>");
                                        if (Convert.ToInt32(dsCU.Tables[0].Rows[i]["Quantity"]) >= Convert.ToInt32(Target))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsCU.Tables[0].Rows[i]["Quantity"] + "</b></font></td>");
                                        }

                                        htmlTable.Append("<td style='color:white;'><b><font face='arial'>" + Convert.ToInt32(Target) + "</font></b></td>");
                                        if (Convert.ToInt32(RunRateShift) >= Convert.ToInt32(ForecastShift))
                                        {
                                            htmlTable.Append("<td><b><font face='arial' color='#64FE2E'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><b><font face='arial' color='red'>" + Convert.ToInt32(RunRateShift) + "</font></b></td>");
                                        }
                                        htmlTable.Append("<td><b><font face='arial'>" + Convert.ToInt32(ForecastShift) + "</font></b></td>");
                                        if (Convert.ToInt32(dsSA.Tables[0].Rows[i]["shift_anterior"]) >= Convert.ToInt32(ForecastShift))
                                        {

                                            htmlTable.Append("<td><b><font face='arial' color='#64FE2E'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                        }
                                        else
                                        {

                                            htmlTable.Append("<td><b><font face='arial' color='red'>" + dsSA.Tables[0].Rows[i]["shift_anterior"] + "</font></b></td>");
                                        }
                                        htmlTable.Append("</tr>");
                                    }

                                }
                                //   htmlTable.Append(@"<tr><td colspan='4' style='text-align:left; border-right:0px;'> Ziua anterioara : </td>
                                //      <td colspan='2' style='text-align:right; border-left:0px;'>" + yesterday + @"</td>    
                                //</tr>");
                                htmlTable.Append(@"<tr style='border-top:2px; border-top-style: solid;'><td>" + yesterday + @" </td>
                                   <td> S1 </td>  
                                   <td> S2 </td>  
                                   <td> S3 </td>  
                                   <td> Total </td>  
                                   <td> Obiectiv </td>  
                                </tr>");
                                if (dsScrapLastDay.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsScrapLastDay.Tables[0].Rows.Count; i++)
                                    {
                                        ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                        //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                        //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                        //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                        //Label1.Text = Convert.ToString(DailyTarget);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'>" + "Scrap IV" + "</font></td>");
                                        if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s1"]) >= Convert.ToInt32(TargetScrapShift))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s1"] + "</b></font></td>");
                                        }
                                        if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s2"]) >= Convert.ToInt32(TargetScrapShift))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s2"] + "</b></font></td>");
                                        }
                                        if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["s3"]) >= Convert.ToInt32(TargetScrapShift))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsScrapLastDay.Tables[0].Rows[i]["s3"] + "</b></font></td>");
                                        };
                                        if (Convert.ToInt32(dsScrapLastDay.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(TargetScrapTotal))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsScrapLastDay.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                        }
                                        htmlTable.Append("<td><b><font face='arial'>" + Convert.ToInt32(TargetScrapTotal) + "</font></b></td>");
                                        htmlTable.Append("</tr>");
                                    }
                                }
                                if (dsLD.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsLD.Tables[0].Rows.Count; i++)
                                    {
                                        ForecastShiftY = Convert.ToDouble(dsFCY.Tables[0].Rows[0]["Forcast"]) / 3;
                                        //RunRateShift = Convert.ToDouble(dsCU.Tables[0].Rows[i]["quantity"]) / minutes * ShiftTotalTime;
                                        //DailyTarget = (Convert.ToDouble(dsCU.Tables[0].Rows[i]["Forecast"]) / 24) / 6;
                                        //Target = DailyTarget * Convert.ToInt32(intervaleTrecute);
                                        //Label1.Text = Convert.ToString(DailyTarget);
                                        htmlTable.Append("<tr>");
                                        htmlTable.Append("<td style='background-color:black; color:white;'><font face='arial'> Volum " + dsLD.Tables[0].Rows[i]["area"] + "</font></td>");
                                        if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_1"]) >= Convert.ToInt32(ForecastShiftY))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_1"] + "</b></font></td>");
                                        }
                                        if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_2"]) >= Convert.ToInt32(ForecastShiftY))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_2"] + "</b></font></td>");
                                        }
                                        if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["shift_3"]) >= Convert.ToInt32(ForecastShiftY))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["shift_3"] + "</b></font></td>");
                                        };
                                        if (Convert.ToInt32(dsLD.Tables[0].Rows[i]["total"]) >= Convert.ToInt32(dsFCY.Tables[0].Rows[0]["Forcast"]))
                                        {
                                            htmlTable.Append("<td><font face='arial' color='#64FE2E'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                        }
                                        else
                                        {
                                            htmlTable.Append("<td><font face='arial' color='red'><b>" + dsLD.Tables[0].Rows[i]["total"] + "</b></font></td>");
                                        }
                                        htmlTable.Append("<td><b><font face='arial'>" + dsFCY.Tables[0].Rows[0]["Forcast"] + "</font></b></td>");
                                        htmlTable.Append("</tr>");
                                    }
                                }

                                ////////////////////

                            }
                        }
                    }
                }
            }

            htmlTable.Append("</tr>");
            htmlTable.Append("</font></table>");
            DBDataPlaceHolder.Controls.Add(new Literal { Text = htmlTable.ToString() });
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }
    }
}