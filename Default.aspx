<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-group">
        <div class="form-horizontal">
            <asp:Label runat="server" AssociatedControlID="Path" CssClass="col-md-2 control-label">Path: </asp:Label>
            <div class="col-md-10">
                <asp:TextBox runat="server" ID="Path" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Path"
                CssClass="text-danger" ErrorMessage="The Path field is required." />
            </div>
            <div class="col-md-offset-2 col-md-10">
                <asp:Button runat="server" OnClick="Start" Text="Start" CssClass="btn btn-default" />
            </div>
            <br/>
            <div class="col-md-offset-2 col-md-10">
                <asp:Label runat="server" AssociatedControlID="Output" ID="Output"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
