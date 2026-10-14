<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ScannerQR.aspx.cs" Inherits="FirstASP.ScannerQR" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://w3.org">
<html xmlns="http://w3.org">
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <script src="Scripts\html5-qrcode.min.js" type="text/javascript"></script>
    <title>Сканирование</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="reader" style="width: 100%; max-width: 500px; min-height: 300px; border: 1px solid #ccc; margin: 0 auto;"></div>
    <div id="debug-log" style="color: red; padding: 10px; font-family: monospace;"></div>
    <asp:HiddenField ID="hfScanResult" runat="server" />
    <asp:Button ID="btnProcessCode" runat="server" OnClick="btnProcessCode_Click" Style="display:none;" />
    <asp:Button ID="btnCancel" runat="server" Text="Отмена" style="width: 100%; max-width: 500px; height: 40px; margin-top: 15px; background-color: #f44336; color: white; border: none; font-size: 16px; cursor: pointer;" onclick="btnCancel_Click" />
    <script type="text/javascript">
        function onScanSuccess(decodedText, decodedResult) {
            document.getElementById('debug-log').style.color = 'green';
            document.getElementById('debug-log').innerHTML = 'Код успешно считан! Возврат...';
            if (typeof html5QrcodeScanner !== 'undefined') html5QrcodeScanner.clear();
            document.getElementById('<%= hfScanResult.ClientID %>').value = decodedText;
            document.getElementById('<%= btnProcessCode.ClientID %>').click();
        }
        if (typeof Html5QrcodeScanner === 'undefined') {
            document.getElementById('debug-log').innerHTML = 'Ошибка: Скрипт в папке Scripts не найден.';
        } else {
            try {
                const config = { fps: 20, qrbox: function(w, h) { const m = Math.min(w, h); return { width: Math.floor(m * 0.75), height: Math.floor(m * 0.75) }; }, formatsToSupport: [ Html5QrcodeSupportedFormats.QR_CODE, Html5QrcodeSupportedFormats.DATA_MATRIX ], experimentalFeatures: { useBarCodeDetectorIfSupported: true } };
                const html5Qrcode = new Html5Qrcode("reader");
                html5Qrcode.start({ facingMode: "environment" }, config, onScanSuccess, function(err) {}).then(() => {
                    document.getElementById('debug-log').style.color = 'gray';
                    document.getElementById('debug-log').innerHTML = 'Сканер активен. Наведите на QR или DataMatrix.';
                }).catch(err => { document.getElementById('debug-log').innerHTML = 'Ошибка камеры: ' + err; });
            } catch (err) { document.getElementById('debug-log').innerHTML = 'Ошибка: ' + err.message; }
        }
    </script>
    </form>
</body>
</html>