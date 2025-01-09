function displayFileNames() {
    const input = document.getElementById('files');
    const fileList = document.getElementById('fileList');
    const files = input.files;

    fileList.innerHTML = '';

    if (files.length > 0) {
        const ul = document.createElement('ul');
        for (let i = 0; i < files.length; i++) {
            const li = document.createElement('li');
            li.textContent = files[i].name;
            ul.appendChild(li);
        }
        fileList.appendChild(ul);
    } else {
        fileList.textContent = 'No files selected.';
    }
}

function showTable(table) {
    var completedTable = document.getElementById('completedTable');
    var notCompletedTable = document.getElementById('notCompletedTable');

    completedTable.classList.add('d-none');
    notCompletedTable.classList.add('d-none');

    if (table === 'completed') {
        completedTable.classList.remove('d-none');
    } else if (table === 'notCompleted') {
        notCompletedTable.classList.remove('d-none');
    }
}