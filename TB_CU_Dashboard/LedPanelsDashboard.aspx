<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LedPanelsDashboard.aspx.cs" Inherits="TB_CU_Dashboard.LedPanelsDashboard" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Led Panels</title>
    <meta http-equiv="Refresh" content="300;url=LedPanelsDashboard.aspx" />
  
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
