// ==================== PROPERTY WIZARD ====================
let currentWizardStep = 1;

function updateWizardProgress(step) {
    const totalSteps = 5;
    const progress = (step / totalSteps) * 100;
    document.getElementById('createPropProgress').style.width = progress + '%';
    document.getElementById('createPropProgress').setAttribute('aria-valuenow', progress);
    
    // Update tabs
    for(let i=1; i<=totalSteps; i++) {
        const tab = document.getElementById('step' + i + '-tab');
        if (i < step) {
            tab.classList.remove('active');
            tab.classList.add('bg-success', 'text-white');
        } else if (i === step) {
            tab.classList.remove('bg-success', 'text-white');
            tab.classList.add('active');
        } else {
            tab.classList.remove('active', 'bg-success', 'text-white');
        }
    }
}

function nextWizardStep(step) {
    // Validate current step before proceeding
    if (step === 2) {
        if (!validateStep1()) return;
    } else if (step === 3) {
        if (!validateStep2()) return;
    } else if (step === 4) {
        if (!validateStep3()) return;
    } else if (step === 5) {
        // Step 4 (Services) no mandatory validation required
    }
    
    currentWizardStep = step;
    const targetTab = new bootstrap.Tab(document.getElementById('step' + step + '-tab'));
    targetTab.show();
    updateWizardProgress(step);
}

function prevWizardStep(step) {
    currentWizardStep = step;
    const targetTab = new bootstrap.Tab(document.getElementById('step' + step + '-tab'));
    targetTab.show();
    updateWizardProgress(step);
}

function markInvalid(inputEl, message) {
    if (!inputEl) return;
    inputEl.classList.add('is-invalid');
    let feedback = inputEl.parentNode.querySelector('.invalid-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        inputEl.parentNode.appendChild(feedback);
    }
    if (message) feedback.textContent = message;
}

function markValid(inputEl) {
    if (!inputEl) return;
    inputEl.classList.remove('is-invalid');
}

function validateStep1() {
    let isValid = true;
    const name = document.getElementById('createPropName');
    if (!name.value.trim()) { markInvalid(name); isValid = false; } else markValid(name);
    
    const phone = document.getElementById('createPropPhone');
    if (!phone.value.trim() || phone.value.trim().length < 9) { markInvalid(phone); isValid = false; } else markValid(phone);
    
    const email = document.getElementById('createPropEmail');
    if (!email.value.trim() || !email.value.includes('@')) { markInvalid(email); isValid = false; } else markValid(email);
    
    const city = document.getElementById('createPropCity');
    if (!city.value.trim()) { markInvalid(city); isValid = false; } else markValid(city);
    
    const address = document.getElementById('createPropAddress');
    if (!address.value.trim()) { markInvalid(address); isValid = false; } else markValid(address);
    
    return isValid;
}

function validateStep2() {
    let isValid = true;
    const doc1 = document.getElementById('createPropBusinessLicense');
    if (!doc1.value) { markInvalid(doc1); isValid = false; } else markValid(doc1);
    
    const doc2 = document.getElementById('createPropFireSafety');
    if (!doc2.value) { markInvalid(doc2); isValid = false; } else markValid(doc2);
    
    const doc3 = document.getElementById('createPropSecurity');
    if (!doc3.value) { markInvalid(doc3); isValid = false; } else markValid(doc3);
    return isValid;
}

function validateStep3() {
    let isValid = true;
    const policy = document.getElementById('createPropPolicy');
    if (!policy.value.trim() || policy.value.length < 20) { markInvalid(policy); isValid = false; } else markValid(policy);
    
    const photos = document.getElementById('createPropPhotos');
    if (!photos.value) { markInvalid(photos); isValid = false; } else markValid(photos);
    return isValid;
}

let serviceCounter = 0;
function addCreatePropertyServiceRow() {
    const container = document.getElementById('createPropServicesContainer');
    document.getElementById('createPropNoServiceText').classList.add('d-none');
    
    const rowId = 'srvRow' + serviceCounter;
    const div = document.createElement('div');
    div.className = 'row g-2 align-items-end mb-3 pb-3 border-bottom';
    div.id = rowId;
    div.innerHTML = `
        <div class="col-md-4">
            <label class="form-label small fw-bold">Tên dịch vụ *</label>
            <input type="text" name="InitialServices[${serviceCounter}].Name" class="form-control rounded-pill" placeholder="Ví dụ: Đưa đón sân bay" required>
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-bold">Giá (₫) *</label>
            <input type="number" name="InitialServices[${serviceCounter}].Price" class="form-control rounded-pill" value="0" min="0" required>
        </div>
        <div class="col-md-4">
            <label class="form-label small fw-bold">Mô tả ngắn</label>
            <input type="text" name="InitialServices[${serviceCounter}].Description" class="form-control rounded-pill" placeholder="...">
        </div>
        <div class="col-md-1 text-end">
            <button type="button" class="btn btn-outline-danger rounded-circle" onclick="document.getElementById('${rowId}').remove()"><i class="bi bi-trash"></i></button>
        </div>
    `;
    container.appendChild(div);
    serviceCounter++;
}

function generateHotelRoomForms() {
    const count = parseInt(document.getElementById('createPropHotelRoomCount').value) || 1;
    const container = document.getElementById('createPropHotelRoomsContainer');
    container.innerHTML = '';
    
    // Copy options from HomestayRoomTypeId
    const typeSelectHtml = document.getElementById('createPropHomestayRoomTypeId').innerHTML;
    
    for(let i=0; i<count; i++) {
        const div = document.createElement('div');
        div.className = 'p-3 bg-white border rounded-4 mb-3';
        div.innerHTML = `
            <h6 class="fw-bold text-navy small mb-3">Phòng ${i+1}</h6>
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Số/Tên phòng *</label>
                    <input type="text" name="InitialRooms[${i}].RoomNumber" class="form-control rounded-pill" value="P${i+1}" required>
                </div>
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Loại phòng</label>
                    <select name="InitialRooms[${i}].RoomTypeId" class="form-select rounded-pill">
                        ${typeSelectHtml}
                    </select>
                </div>
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Giá 1 đêm (₫) *</label>
                    <input type="number" name="InitialRooms[${i}].Price" class="form-control rounded-pill" value="500000" min="0" required>
                </div>
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Người lớn *</label>
                    <input type="number" name="InitialRooms[${i}].AdultCapacity" class="form-control rounded-pill" value="2" min="1" required>
                </div>
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Trẻ em</label>
                    <input type="number" name="InitialRooms[${i}].ChildCapacity" class="form-control rounded-pill" value="1" min="0">
                </div>
                <div class="col-md-4">
                    <label class="form-label small fw-bold">Mô tả (tùy chọn)</label>
                    <input type="text" name="InitialRooms[${i}].Description" class="form-control rounded-pill">
                </div>
            </div>
        `;
        container.appendChild(div);
    }
}

function submitCreatePropertyForm(e) {
    e.preventDefault();
    const type = document.getElementById('createPropTypeSelect').value;
    
    if (type === 'Homestay') {
        const price = document.getElementById('createPropHomestayPrice');
        if (!price.value || price.value < 50000) { markInvalid(price); return; }
    } else {
        const count = document.getElementById('createPropHotelRoomCount').value;
        if (count < 1) { alert("Cần ít nhất 1 phòng"); return; }
    }
    
    // Check if form is completely valid based on browser API
    const form = document.getElementById('createPropertyForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    // Submit
    form.submit();
}

document.addEventListener('DOMContentLoaded', () => {
    // initialize first room form
    generateHotelRoomForms();
    
    // Auto-remove invalid markings
    document.querySelectorAll('#createPropertyForm input, #createPropertyForm textarea').forEach(el => {
        el.addEventListener('input', () => { if(el.value.trim()) markValid(el); });
    });
});
