// Открывает окно просмотра документа.
function openDocument(pdfUrl, documentId, isReviewed) {
    const pdfIframe = document.getElementById('pdf-iframe');
    pdfIframe.src = pdfUrl;

    $('#document-id').val(documentId);

    if (isReviewed === null) {
        $('#agreement-group').hide();
    }
    else {
        $('#agreement-checkbox').prop("checked", isReviewed);
    }

    $('#pdf-modal').modal('show');
}

// Функция отображения вкладок для документов.
function showCategoryTabs() {
    $.ajax({
        url: "/DocumentView/GetCategories",
        type: "GET",
        success: function (response) {
            const tabs = $('#categoryTabs');
            tabs.html(response);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

// Функция открытия модального окна.
function openModal(url, id) {
    $.ajax({
        url: url,
        type: "GET",
        data: { id: id },
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

// Функция для изменения записи по идентификатору.
function editItem(controllerName, id) {
    openModal('/' + controllerName + '/GetItem', id);
}

// Функция для удаления записи по идентификатору.
function deleteItem(controllerName, id) {
    if (confirm("Вы уверены, что хотите удалить эту запись?")) {
        $.ajax({
            url: '/' + controllerName + '/DeleteItem',
            type: "POST",
            data: { id: id },
            success: function () {
                alert("Запись успешно удалена.");
                // Перезагружаем страницу
                location.reload();
            },
            error: function (error) {
                console.log(error);
                alert("Произошла ошибка при удалении записи.");
            }
        });
    }
}

// Функция для проверки данных на уникальность.
function checkUnique(controllerName, excludedFields = []) {
    let fieldTags = ['input', 'textarea', 'select'];
    let formData = {};

    fieldTags.forEach(function (tag) {
        $('form ' + tag).each(function () {
            let fieldName = $(this).attr('name');

            // Проверяем, что имя поля не содержится в исключенных полях
            if (!excludedFields.includes(fieldName)) {
                formData[fieldName] = $(this).val();
            }
        });
    });

    let formAlert = $('#formAlert');
    let data = JSON.stringify(formData);

    $.ajax({
        url: '/' + controllerName + '/CheckUnique',
        type: 'POST',
        contentType: 'application/json',
        data: data,
        success: function (result) {
            if (result.isUnique && result.isValid) {
                formAlert.hide();
                $('form').unbind('submit').submit();
            } else if (!result.isValid) {
                formAlert.text('Поля неправильно заполнены!');
                formAlert.show();
            } else if (!result.isUnique) {
                formAlert.text('Запись с такими данными уже существует!');
                formAlert.show();
            }
        },
        error: function (error) {
            console.log(error);
            alert("Произошла ошибка при отправке данных.");
        }
    });
}

// Функция для конвертации bool.
function convertToJSBool(csharpBool) {
    if (!csharpBool) return false;

    // Преобразование строки в нижний регистр и сравнение с "true"
    return csharpBool.toLowerCase() === "true";
}

// Получает перевод сообщений в таблице.
function getRussianDataTableTranslation() {
    return {
        "decimal":        "",
        "emptyTable":     "Нет данных для отображения",
        "info":           "Показано с _START_ по _END_ из _TOTAL_ записей",
        "infoEmpty":      "Показано 0 из 0 записей",
        "infoFiltered":   "(отфильтровано из _MAX_ записей)",
        "infoPostFix":    "",
        "thousands":      ",",
        "lengthMenu":     "Показать _MENU_ записей",
        "loadingRecords": "Загрузка...",
        "processing":     "Обработка...",
        "search":         "Поиск:",
        "zeroRecords":    "Совпадающих записей не найдено",
        "paginate": {
            "first":      "Первая",
            "last":       "Последняя",
            "next":       "Следующая",
            "previous":   "Предыдущая"
        },
        "aria": {
            "sortAscending":  ": активировать для сортировки столбца по возрастанию",
            "sortDescending": ": активировать для сортировки столбца по убыванию"
        }
    };
}

// Настраивает таблицу для отображения на странице.
function configureDataTable(columnIndexDisableSort) {
    // Настраиваем параметры таблицы
    const table = $('#dataTable').DataTable( {
        info: false,
        ordering: true,
        paging: false,
        stateSave: true,
        language: getRussianDataTableTranslation(),
        columnDefs: [ {
            targets: columnIndexDisableSort,
            orderable: false
        } ]
    } );

    const searchInput = $('#searchInput');

    // Устанавливаем значение поля поиска
    searchInput.val($('#dataTable_filter input').val());

    // Скрываем поиск по умолчанию
    $('#dataTable_filter').hide();

    // Устанавливаем событие для поля поиска
    searchInput.on( 'input', function () {
        table.search( this.value ).draw();
    } );

    // Устанавливаем событие для кнопки очистки поля поиска
    $('#clearButton').on( 'click', function () {
        searchInput.val('').focus();
        table.search('').draw();
    } );
}