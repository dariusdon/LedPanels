<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LedPanelsV4.aspx.cs" Inherits="TB_CU_Dashboard.LedPanelsV4" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Led Panels</title>
     <meta http-equiv="Refresh" content="300;url=LedPanelsV4.aspx" />
</head>
<body style="background-color: black; color: white;">
    <form id="form1" runat="server">
       <div>
            <table>
                <asp:PlaceHolder ID="DBDataPlaceHolder" runat="server"></asp:PlaceHolder> 
            </table>
        </div>
        
        <p>
            &nbsp;</p>
        <p>
            &nbsp;</p>
        <p>
            &nbsp;</p>
        <p>
            <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
        </p>
        <p>
            <br />
            <asp:Label ID="Label3" runat="server" Text="Label"></asp:Label>
        </p><br /><br />
        <asp:Button ID="Button1" runat="server" Text="Back to main page" OnClick="Button1_Click" />
    </form>
</body>
</html>
