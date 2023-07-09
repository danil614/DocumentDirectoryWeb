// Открывает окно просмотра документа.
function openDocument(pdfUrl) {
    const pdfIframe = document.getElementById('pdf-iframe');
    pdfIframe.src = pdfUrl;

    $('#pdf-modal').modal('show');
}

// Функция открытия модального окна.
function openModal(url, id) {
    $.ajax({
        url: url,
        type: "GET",
        data: {id: id},
        success: function (response) {
            let modal = $('#modalWindow');
            modal.html(response);
            modal.modal('show');
        },
        error: function (error) {
            console.log(error);
            alert("Произошла ошибка при загрузке данных.");
        }
    });
}

// Функция для создания новой записи.
function createItem(controllerName) {
    openModal('/' + controllerName + '/CreateItem', '-1');
}

// Функция для поиска в таблице
function filterTable() {
    let searchText = $("#searchInput")
        .val().trim().toLowerCase();
    let rows = $("#dataTable tbody tr");

    rows.each(function () {
        let row = $(this);
        let showRow = false;
        row.find("td").each(
            function () {
                let cellText = $(this).text().toLowerCase();
                if (cellText.includes(searchText)) {
                    showRow = true;
                    return false;
                }
            });
        if (showRow) {
            row.show();
        } else {
            row.hide();
        }
    });
}

// Функция для очистки поля поиска.
function clearInputFilter() {
    $("#searchInput").val("").focus();
    filterTable();
}