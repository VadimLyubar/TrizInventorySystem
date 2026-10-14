<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FirstASP.DefaultMenu" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://w3.org">
<html xmlns="http://w3.org">
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>Главное меню</title>
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background-color: #f3f4f6; margin: 0; padding: 20px;">
    <form id="form1" runat="server">
    <div style="max-width: 500px; margin: 50px auto; text-align: center; background: white;
        padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);
        box-sizing: border-box;">
        <h2 style="color: #1f2937; font-size: 22px; font-weight: 700; margin-top: 0; margin-bottom: 10px;">
            Учет имущества TrizDB
        </h2>
        <p style="color: #6b7280; font-size: 14px; margin-bottom: 35px;">
            Универсальный терминал сбора данных. Выберите операцию:
        </p>
        <asp:Button ID="btnGoCard" runat="server" Text="📝 Карточка объекта (Учет / Просмотр)"
            OnClick="btnGoCard_Click" Style="width: 100%; height: 55px; background-color: #2563eb;
            color: white; border: none; border-radius: 6px; font-size: 16px; font-weight: 600;
            cursor: pointer; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(37,99,235,0.2);" />
        <asp:Button ID="btnGoMove" runat="server" Text="🔄 Перемещение объекта (Кабинет)"
            OnClick="btnGoMove_Click" Style="width: 100%; height: 55px; background-color: #4b5563;
            color: white; border: none; border-radius: 6px; font-size: 16px; font-weight: 600;
            cursor: pointer; box-shadow: 0 2px 4px rgba(75,85,99,0.2);" />
    </div>
    </form>
</body>
</html>
