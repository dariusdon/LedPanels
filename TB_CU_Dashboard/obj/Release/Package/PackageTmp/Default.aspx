<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TB_CU_Dashboard.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TB & CU Dashboard</title>
    <meta charset="utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <meta name="viewport" content="width=device-width, initial-scale=1"/>
    <meta http-equiv="refresh" content="300"/> 
    <link rel="icon" href="images/horse.png">
</head>
<body style="background-color: black; color: white;">
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="Button1" runat="server" Text="Led Panels" OnClick="Button1_Click" />  
            <asp:Button ID="Button2" runat="server" Text="TB & CU" OnClick="Button2_Click" />            
            <asp:Button ID="Button3" runat="server" Text="Led Panels V4" OnClick="Button3_Click" /> 
            <asp:Button ID="Button4" runat="server" Text="Led Panels Dashboard (TV)" OnClick="Button4_Click" />
        </div>
    </form>
</body>

</html>
