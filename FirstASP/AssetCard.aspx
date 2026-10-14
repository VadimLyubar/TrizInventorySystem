<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AssetCard.aspx.cs" Inherits="FirstASP.AssetCard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://w3.org">
<html xmlns="http://w3.org">
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>Карточка объекта имущества</title>
</head>
<body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 0; padding: 0; background-color: #f9fafb;">
    <form id="form1" runat="server" enctype="multipart/form-data">
    
    <div style="padding: 20px; box-sizing: border-box; max-width: 600px; margin: 0 auto;">
        
        <h2 style="color: #111827; font-size: 20px; font-weight: 700; margin-top: 0; margin-bottom: 20px; border-bottom: 2px solid #e5e7eb; padding-bottom: 10px;">
            Карточка объекта имущества
        </h2>

        <!-- УНИВЕРСАЛЬНЫЙ ВЕРХНИЙ СИСТЕМНЫЙ ИНФОРМЕР -->
        <asp:Panel ID="pnlSystemInformer" runat="server" Visible="false" Style="width: 100%; box-sizing: border-box; margin: 10px 0 20px 0; padding: 12px 15px; border-radius: 4px; display: flex; align-items: center; border: 1px solid #ccc;">
            <span style="font-size: 18px; margin-right: 10px; font-weight: bold; line-height: 1;"><asp:Literal ID="litInformerIcon" runat="server"></asp:Literal></span>
            <asp:Label ID="lblInformerText" runat="server" Style="font-size: 14px; font-family: sans-serif; font-weight: 500; line-height: 1.4;"></asp:Label>
        </asp:Panel>

        <!-- БЛОК СКАНИРОВАНИЯ QR -->
        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Шаг 1. Отсканируйте уникальный QR-код:</div>
        <div style="display: flex; width: 100%; align-items: center; margin-bottom: 20px;">
            <asp:TextBox ID="TextBoxQR" runat="server" placeholder="Нажмите на кнопку справа 📷..." Style="flex-grow: 1; height: 40px; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; background-color: #f3f4f6;" ReadOnly="True"></asp:TextBox>
            <asp:Button ID="ButtonQR" runat="server" Text="📷" OnClick="ButtonQR_Click" Style="flex-shrink: 0; width: 55px; height: 40px; margin-left: 8px; font-size: 18px; cursor: pointer; border: 1px solid #d1d5db; border-radius: 4px; background-color: #ffffff;" OnClientClick="preparePhotosForServer();" />
        </div>

        <!-- ДИНАМИЧЕСКИЙ БЛОК ПОЛЕЙ -->
        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Инвентарный номер (бухгалтерский):</div>
        <div style="width: 100%; margin-bottom: 15px;">
            <asp:TextBox ID="TextBoxInventoryNumber" runat="server" inputmode="numeric" Enabled="false" placeholder="Ожидание сканирования..." Style="width: 100%; height: 40px; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box"></asp:TextBox>
        </div>

        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Наименование объекта имущества:</div>
        <div style="width: 100%; margin-bottom: 15px;">
            <asp:TextBox ID="TextBoxName" runat="server" Enabled="false" placeholder="Ожидание сканирования..." Style="width: 100%; height: 40px; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box"></asp:TextBox>
        </div>

        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Выбор материально ответственного лица (МОЛ):</div>
        <div style="width: 100%; margin-bottom: 15px;">
            <asp:DropDownList ID="ddlResponsiblePerson" runat="server" Enabled="false" Style="width: 100%; height: 40px; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; background-color:#ffffff;"></asp:DropDownList>
        </div>

        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Местонахождение (Кабинет / Склад):</div>
        <div style="width: 100%; margin-bottom: 15px;">
            <asp:TextBox ID="TextBoxLocation" runat="server" TextMode="MultiLine" Rows="3" Enabled="false" placeholder="Ожидание сканирования..." Style="width: 100%; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; font-family:sans-serif;"></asp:TextBox>
        </div>

        <div style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Временные пользователи:</div>
        <div style="width: 100%; margin-bottom: 25px;">
            <asp:TextBox ID="TextBoxUsers" runat="server" Enabled="false" placeholder="Ожидание сканирования..." Style="width: 100%; height: 40px; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box"></asp:TextBox>
        </div>

        <!-- ГАРАНТИРОВАННЫЙ НИЗ ФОРМЫ С ТРИЗ-РАЗДЕЛЕНИЕМ ФУНКЦИЙ КАМЕРЫ И ГАЛЕРЕИ -->
        <div id="divPhotoLabel" runat="server" style="margin-bottom: 5px; color: #374151; font-size: 14px; font-weight: 600;">Фотографии объекта имущества:</div>
        <div id="divPhotoButtons" runat="server" style="display: none; gap: 10px; width: 100%; margin-bottom: 10px; box-sizing: border-box;">
            
            <label style="flex: 1; display: flex; align-items: center; justify-content: center; height: 45px; background-color: #3b82f6; color: white; border-radius: 6px; font-size: 15px; font-weight: 600; cursor: pointer; text-align: center; box-shadow: 0 1px 2px rgba(0,0,0,0.05);">
                📸 Сделать снимок
                <input type="file" id="inputCameraOnly" accept="image/*" capture="environment" style="display: none;" />
            </label>

            <label style="flex: 1; display: flex; align-items: center; justify-content: center; height: 45px; background-color: #6b7280; color: white; border-radius: 6px; font-size: 15px; font-weight: 600; cursor: pointer; text-align: center; box-shadow: 0 1px 2px rgba(0,0,0,0.05);">
                📁 Из галереи
                <input type="file" id="inputGalleryOnly" multiple="multiple" accept="image/*" style="display: none;" />
            </label>
        </div>

        <div id="photosPreviewContainer" style="display: flex; flex-wrap: wrap; gap: 10px; margin-bottom: 25px; width: 100%;"></div>

        <!-- Поле скрытого хранения состояния для JS-рендеринга -->
        <asp:HiddenField ID="hfCachedPhotosBase64" runat="server" />
        <asp:HiddenField ID="hfIsReadOnlyMode" runat="server" Value="false" />

        <div style="width: 100%;">
            <asp:Button ID="ButtonSaveAsset" runat="server" Text="Сохранить изменения" Enabled="false" Style="width: 100%; height: 45px; background-color: #4b5563; color: white; border: none; border-radius: 6px; font-size: 16px; font-weight: 600; cursor: pointer;" onclick="ButtonSaveAsset_Click" OnClientClick="preparePhotosForServer();" />
        </div>
        
    </div>
    </form>

    <script type="text/javascript">
        let accumulatedPhotos = [];
        
        window.onload = function() { 
            restorePhotosFromServer(); 
        };
        
        function restorePhotosFromServer() {
            const hf = document.getElementById('hfCachedPhotosBase64');
            if (!hf || !hf.value) return;
            const imgs = hf.value.split('|||');
            for (let i = 0; i < imgs.length; i++) {
                if (imgs[i]) { 
                    accumulatedPhotos.push(imgs[i]); 
                    createPhotoThumbnail(imgs[i], accumulatedPhotos.length - 1); 
                }
            }
        }

        function handleIncomingFiles(files) {
            if (!files) return;
            for (let i = 0; i < files.length; i++) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    compressImageOnFly(e.target.result, function(compressedBase64) {
                        accumulatedPhotos.push(compressedBase64);
                        createPhotoThumbnail(compressedBase64, accumulatedPhotos.length - 1);
                        preparePhotosForServer();
                    });
                };
                reader.readAsDataURL(files[i]);
            }
        }

        // CANVAS-СЖАТИЕ: Ограничение длинной стороны в 1280px и сжатие в 70% JPEG
        function compressImageOnFly(base64Src, callback) {
            const img = new Image();
            img.src = base64Src;
            img.onload = function() {
                const canvas = document.createElement('canvas');
                let width = img.width; let height = img.height;
                const max_size = 1280;

                if (width > height) {
                    if (width > max_size) { height *= max_size / width; width = max_size; }
                } else {
                    if (height > max_size) { width *= max_size / height; height = max_size; }
                }
                canvas.width = width; canvas.height = height;
                
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);
                
                const dataUrl = canvas.toDataURL('image/jpeg', 0.7);
                callback(dataUrl);
            };
        }

        const camInput = document.getElementById('inputCameraOnly');
        if(camInput) { camInput.addEventListener('change', function(e) { handleIncomingFiles(e.target.files); this.value = ''; }); }
        
        const galInput = document.getElementById('inputGalleryOnly');
        if(galInput) { galInput.addEventListener('change', function(e) { handleIncomingFiles(e.target.files); this.value = ''; }); }

        function createPhotoThumbnail(src, idx) {
            const container = document.getElementById('photosPreviewContainer');
            const isReadOnly = document.getElementById('hfIsReadOnlyMode').value === "true";
            const wrp = document.createElement('div'); wrp.id = 'thumb_' + idx;
            wrp.style = "position:relative; width:80px; height:80px; border:1px solid #ddd; border-radius:4px; overflow:hidden;";
            const img = document.createElement('img'); img.src = src; img.style = "width:100%; height:100%; object-fit:cover;";
            wrp.appendChild(img);
            // ТРИЗ-ЗАЩИТА: Прячем крестики удаления фото, если карточка в режиме Просмотра
            if (!isReadOnly) {
                const cross = document.createElement('div'); cross.innerHTML = '×';
                cross.style = "position:absolute; top:2px; right:2px; width:18px; height:18px; background:rgba(240,82,82,0.8); color:white; border-radius:50%; text-align:center; line-height:16px; cursor:pointer; font-weight:bold; font-family:sans-serif;";
                cross.onclick = function() { wrp.remove(); accumulatedPhotos[idx] = null; preparePhotosForServer(); };
                wrp.appendChild(cross);
            }
            container.appendChild(wrp);
        }

        function preparePhotosForServer() {
            const valid = accumulatedPhotos.filter(p => p !== null);
            const hf = document.getElementById('hfCachedPhotosBase64');
            if(hf) hf.value = valid.join('|||');
        }

    </script>
</body>
</html>
