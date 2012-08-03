<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebApp._Default" EnableEventValidation="true" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <link href="Styles/Style.css" rel="stylesheet" type="text/css" />
    <script src="Scripts/jquery-1.4.1.js" type="text/javascript"></script>
    <script type="text/javascript">
        var rptTable_ClientID = '#<%=this.rptTable.ClientID %>';
        $(function () {
            var page = 1;
            var i = 5;
            $('span.next').click(function () {
                var $parent = $(this).parents('div.v_show');
                var $v_content = $parent.find('div.v_content');
                var $v_show = $v_content.find('div.v_content_list');


                var v_width = $v_content.width();
                var len = $v_show.find('table tr th').length;
                var page_count = Math.ceil(len / i);
                if (!$v_show.is(":animated")) {
                    if (page == page_count) {
                        return;
                    }
                    else {
                        $v_show.animate({ left: '-=' + v_width }, 'slow');
                        page++;
                    }
                }
            });
            $('table.tb_custom').eq(0).clone().insertAfter($('table.tb_custom:last'));
            alert($('table.tb_custom').length);

        });
    </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Welcome to ASP.NET!
    </h2>
    <p>
        To learn more about ASP.NET visit <a href="http://www.asp.net" title="ASP.NET Website">
            www.asp.net</a>.
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>
    <asp:Repeater ID="rptTable" runat="server" OnItemCommand="rptTable_ItemCommand">
        <HeaderTemplate>
            <table>
                <tr>
                    <th>Delete
                    </th>
                    <th>
                        A
                    </th>
                    <th>
                        B
                    </th>
                    <th>
                        C
                    </th>
                </tr>
            </table>
        </HeaderTemplate>
        <ItemTemplate>
            <table class="tb_custom">
                <tr>
                    <td><asp:Button runat="server" CommandName="delete" Text="Del" /> </td>
                    <td>
                        <%#Eval("A") %>
                    </td>
                    <td>
                        <%#Eval("B") %>
                    </td>
                    <td>
                        <%#Eval("C") %>
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    <div class="v_show">
        <div class="v_caption">
            <div class="change_btn">
                <span class="prev">Previous</span> <span class="next">Next</span>
            </div>
        </div>
        <div class="v_content">
            <div class="v_content_list">
                <table>
                    <tr>
                        <th>
                            1
                        </th>
                        <th>
                            2
                        </th>
                        <th>
                            3
                        </th>
                        <th>
                            4
                        </th>
                        <th>
                            5
                        </th>
                        <th>
                            6
                        </th>
                        <th>
                            7
                        </th>
                        <th>
                            8
                        </th>
                        <th>
                            9
                        </th>
                    </tr>
                    <tr>
                        <td>
                            1
                        </td>
                        <td>
                            2
                        </td>
                        <td>
                            3
                        </td>
                        <td>
                            4
                        </td>
                        <td>
                            5
                        </td>
                        <td>
                            6
                        </td>
                        <td>
                            7
                        </td>
                        <td>
                            8
                        </td>
                        <td>
                            9
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
