<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="jquery-1.9.0.min.js"></script>
    <script type="text/javascript" src="js/translationsTNG.js"></script>
    <script type="text/javascript" src="js/account-translations.js"></script>  
    <script type="text/javascript" src="cookies.js"></script>
    <link href="labelstyle.css" rel="stylesheet" />

</head>
<body>
    <form id="form1" runat="server">
        <div id="Header">
        <asp:HiddenField ID="hdfPartNo" runat="server"/>
        <asp:Panel ID="pnlHeading" runat="server">
            <table style="position: relative; width:890px" cellspacing="0" align="center">
                <tr>
                    <td align="left" style="width: 100%;">
                        <table style="text-align: left; width: 100%">
                            <tr>
                                <td colspan="5" class="tabletop">
                                    <asp:Label ID="Label14" runat="server" Text="Measures Conversions" />
                                </td>
                            </tr>
                            <tr>
                                <td class="tablecell" style="width:80px">
                                    <asp:Label ID="Label27" runat="server" Text="Qty" CssClass="text8" /></td>
                                <td class="tablecell" style="width:100px">
                                    <asp:Label ID="Label28" runat="server" Text="UOM1" CssClass="text8" /></td>
                                <td class="tablecell" style="width:80px">
                                    <asp:Label ID="Label1" runat="server" Text="Qty" CssClass="text8" /></td>
                                <td class="tablecell" style="width:100px">
                                    <asp:Label ID="Label2" runat="server" Text="UOM2" CssClass="text8" /></td>
                                <td></td>
                            </tr>

                            <asp:Repeater ID="rptDSPItems" runat="server" OnItemCommand="rptDSPItems_ItemCommand" >
                                <ItemTemplate>
                                    <tr style="background-color: #E5F3FF;">
                                        <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                            <asp:Label ID="Label4" Text="1" runat="server" CssClass="text8" Enabled="false"/></td>
                                        <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                            <asp:Label ID="Label3" Text='<%#Eval("MEASURE_FROM")%>' runat="server" CssClass="text8" Enabled="false"/></td>
                                        <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                            <asp:Label ID="lblValue" Text='<%#Eval("VALUE")%>' runat="server" CssClass="text8" Enabled="false"/></td>
                                        <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                            <asp:Label ID="lblOH_UOM" Text='<%#Eval("MEASURE_TO")%>' runat="server" CssClass="text8" Enabled="false"/></td>
                                        <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                            <asp:ImageButton ID="imgDel" ImageUrl="images\delete.png" CommandName="Del" CommandArgument='<%#Eval("MEASURE_ID")%>' runat="server" CssClass="text8" /></td>
                                        <td>
                                            <asp:HiddenField runat="server" />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            <tr>
                                <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                    <asp:Label ID="txtValue_From" runat="server" Text="1" Enabled="false" CssClass="text8box" Width="80px" /></td>
                                <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                    <asp:DropDownList ID="ddlOH_UOM_From" runat="server" CssClass="text8box noTranslate" OnSelectedIndexChanged="ddlOH_UOM_From_SelectedIndexChanged" /></td>
                                <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                    <asp:TextBox ID="txtValue_To" runat="server" CssClass="text8box" Width="80px"/></td>
                                <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                    <asp:DropDownList ID="ddlOH_UOM_To" runat="server" CssClass="text8box noTranslate" /></td>
                                <td style="border-bottom: none; border-top: none; border-right: solid 1px lightgray;">
                                    <asp:Button ID="btnAddItem" runat="server" Text="Add" CssClass="text8" OnClick="btnAddItem_Click" /></td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
    </form>
</body>
</html>