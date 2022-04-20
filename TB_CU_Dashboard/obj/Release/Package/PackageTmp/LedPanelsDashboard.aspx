<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LedPanelsDashboard.aspx.cs" Inherits="TB_CU_Dashboard.LedPanelsDashboard" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Led Panels</title>
    <meta charset="utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <meta http-equiv="refresh" content="300"/> 
    <link rel="icon" href="images/horse.png">
</head>
<body style="background-color: black; color: white;">
    <form id="form1" runat="server">
       <div><center>  
           <table>
                <asp:PlaceHolder ID="DBDataPlaceHolder" runat="server"></asp:PlaceHolder>

                  
            </table>
           </center>
    
        </div>
        
    </form>
</body>
</html>
