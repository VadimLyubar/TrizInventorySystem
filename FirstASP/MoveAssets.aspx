<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MoveAssets.aspx.cs" Inherits="FirstASP.MoveAssets" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://w3.org">
<html xmlns="http://w3.org">
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>Перемещение объекта</title>
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background-color: #f9fafb; padding: 20px;">
    <form id="form1" runat="server">
    <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 6px; box-shadow:0 1px 3px rgba(0,0,0,0.1); box-sizing: border-box;">
        
        <h2 style="margin-top:0; border-bottom:1px solid #ddd; padding-bottom:10px; color:#111827; font-size:18px;">
            Фиксация перемещения объекта
        </h2>

        <!-- ВЕРХНИЙ ИНФОРМЕР С ВЛОЖЕННЫМ СЕРВЕРНЫМ ЛЕЙБЛОМ -->
        <asp:Panel ID="pnlInformer" runat="server" Visible="false" Style="padding:12px; margin-bottom:15px; border-radius:4px; font-weight:500; border:1px solid #ccc; display:flex; align-items:center;">
            <asp:Label ID="lblMoveInformerText" runat="server" Style="font-size: 14px; line-height: 1.4;"></asp:Label>
        </asp:Panel>

        <div style="margin-bottom:5px; font-weight:600; color:#374151; font-size:14px;">Отсканируйте QR-код объекта для верификации:</div>
        <div style="display:flex; margin-bottom:15px; width:100%;">
            <asp:TextBox ID="txtMoveQR" runat="server" Style="flex-grow:1; height:40px; padding:8px; font-size:16px; border:1px solid #ccc; border-radius:4px; background-color:#f3f4f6; box-sizing:border-box;" ReadOnly="true"></asp:TextBox>
            <asp:Button ID="btnScanMove" runat="server" Text="📷" OnClick="btnScanMove_Click" Style="width:55px; height:40px; margin-left:8px; font-size:18px; cursor:pointer; border:1px solid #d1d5db; border-radius:4px;" />
        </div>

        <!-- ТРЕХСТРОЧНОЕ ПОЛЕ ДЛЯ ЧТЕНИЯ ТЕКУЩЕЙ ЛОКАЦИИ (ТРЕБОВАНИЕ ТРИЗ) -->
        <div style="margin-bottom:5px; font-weight:600; color:#374151; font-size:14px;">Действующее (последнее зарегистрированное) местоположение:</div>
        <asp:TextBox ID="TextBoxCurrentLocation" runat="server" TextMode="MultiLine" Rows="3" ReadOnly="true" Style="width:100%; box-sizing:border-box; padding:8px; font-size:16px; margin-bottom:15px; background-color:#f3f4f6; border:1px solid #ccc; border-radius:4px; color:#4b5563; font-family:sans-serif;"></asp:TextBox>

        <div style="margin-bottom:5px; font-weight:600; color:#374151; font-size:14px;">Внесите НОВОЕ местонахождение объекта (Кабинет/Склад):</div>
        <asp:TextBox ID="txtNewLocation" runat="server" TextMode="MultiLine" Rows="3" Enabled="false" placeholder="Например: Кабинет 405, отдел кадров..." Style="width:100%; box-sizing:border-box; padding:8px; font-size:16px; margin-bottom:20px; border:1px solid #ccc; border-radius:4px; font-family:sans-serif;"></asp:TextBox>

        <asp:Button ID="btnSubmitMove" runat="server" Text="Зафиксировать перемещение объекта" Enabled="false" OnClick="btnSubmitMove_Click" Style="width:100%; height:45px; font-size:16px; font-weight:600; color:white; background-color:#4b5563; border:none; border-radius:4px; cursor:pointer;" />
    </div>
    </form>
</body>
</html>