/**
 * Stayly - Vietnam Administrative Divisions & Autocomplete Service
 * Connects to open-api.vn with resilient offline-ready local dataset for 63 provinces/cities.
 */

(function (window) {
    const VIETNAM_PROVINCES = [
        { code: 1, name: "Hà Nội", type: "Thành phố Trung ương", shortName: "Hà Nội" },
        { code: 79, name: "Thành phố Hồ Chí Minh", type: "Thành phố Trung ương", shortName: "Hồ Chí Minh" },
        { code: 48, name: "Thành phố Đà Nẵng", type: "Thành phố Trung ương", shortName: "Đà Nẵng" },
        { code: 68, name: "Tỉnh Lâm Đồng", type: "Tỉnh", shortName: "Lâm Đồng", hotSpots: ["Đà Lạt", "Bảo Lộc"] },
        { code: 56, name: "Tỉnh Khánh Hòa", type: "Tỉnh", shortName: "Khánh Hòa", hotSpots: ["Nha Trang", "Cam Ranh"] },
        { code: 77, name: "Tỉnh Bà Rịa - Vũng Tàu", type: "Tỉnh", shortName: "Vũng Tàu", hotSpots: ["Vũng Tàu", "Bà Rịa", "Côn Đảo"] },
        { code: 91, name: "Tỉnh Kiên Giang", type: "Tỉnh", shortName: "Kiên Giang", hotSpots: ["Phú Quốc", "Hà Tiên", "Rạch Giá"] },
        { code: 49, name: "Tỉnh Quảng Nam", type: "Tỉnh", shortName: "Quảng Nam", hotSpots: ["Hội An", "Tam Kỳ"] },
        { code: 46, name: "Tỉnh Thừa Thiên Huế", type: "Tỉnh", shortName: "Huế", hotSpots: ["Huế"] },
        { code: 22, name: "Tỉnh Quảng Ninh", type: "Tỉnh", shortName: "Quảng Ninh", hotSpots: ["Hạ Long", "Cô Tô", "Vân Đồn"] },
        { code: 10, name: "Tỉnh Lào Cai", type: "Tỉnh", shortName: "Lào Cai", hotSpots: ["Sa Pa", "Bắc Hà"] },
        { code: 37, name: "Tỉnh Ninh Bình", type: "Tỉnh", shortName: "Ninh Bình", hotSpots: ["Tràng An", "Tam Cốc"] },
        { code: 60, name: "Tỉnh Bình Thuận", type: "Tỉnh", shortName: "Bình Thuận", hotSpots: ["Phan Thiết", "Mũi Né"] },
        { code: 52, name: "Tỉnh Bình Định", type: "Tỉnh", shortName: "Bình Định", hotSpots: ["Quy Nhơn"] },
        { code: 54, name: "Tỉnh Phú Yên", type: "Tỉnh", shortName: "Phú Yên", hotSpots: ["Tuy Hòa"] },
        { code: 92, name: "Thành phố Cần Thơ", type: "Thành phố Trung ương", shortName: "Cần Thơ" },
        { code: 31, name: "Thành phố Hải Phòng", type: "Thành phố Trung ương", shortName: "Hải Phòng", hotSpots: ["Cát Bà", "Đồ Sơn"] },
        { code: 24, name: "Tỉnh Bắc Giang", type: "Tỉnh", shortName: "Bắc Giang" },
        { code: 6, name: "Tỉnh Bắc Kạn", type: "Tỉnh", shortName: "Bắc Kạn" },
        { code: 95, name: "Tỉnh Bạc Liêu", type: "Tỉnh", shortName: "Bạc Liêu" },
        { code: 27, name: "Tỉnh Bắc Ninh", type: "Tỉnh", shortName: "Bắc Ninh" },
        { code: 83, name: "Tỉnh Bến Tre", type: "Tỉnh", shortName: "Bến Tre" },
        { code: 74, name: "Tỉnh Bình Dương", type: "Tỉnh", shortName: "Bình Dương" },
        { code: 70, name: "Tỉnh Bình Phước", type: "Tỉnh", shortName: "Bình Phước" },
        { code: 96, name: "Tỉnh Cà Mau", type: "Tỉnh", shortName: "Cà Mau" },
        { code: 4, name: "Tỉnh Cao Bằng", type: "Tỉnh", shortName: "Cao Bằng" },
        { code: 66, name: "Tỉnh Đắk Lắk", type: "Tỉnh", shortName: "Đắk Lắk", hotSpots: ["Buôn Ma Thuột"] },
        { code: 67, name: "Tỉnh Đắk Nông", type: "Tỉnh", shortName: "Đắk Nông" },
        { code: 11, name: "Tỉnh Điện Biên", type: "Tỉnh", shortName: "Điện Biên" },
        { code: 75, name: "Tỉnh Đồng Nai", type: "Tỉnh", shortName: "Đồng Nai" },
        { code: 87, name: "Tỉnh Đồng Tháp", type: "Tỉnh", shortName: "Đồng Tháp" },
        { code: 64, name: "Tỉnh Gia Lai", type: "Tỉnh", shortName: "Gia Lai", hotSpots: ["Pleiku"] },
        { code: 2, name: "Tỉnh Hà Giang", type: "Tỉnh", shortName: "Hà Giang", hotSpots: ["Đồng Văn"] },
        { code: 35, name: "Tỉnh Hà Nam", type: "Tỉnh", shortName: "Hà Nam" },
        { code: 42, name: "Tỉnh Hà Tĩnh", type: "Tỉnh", shortName: "Hà Tĩnh" },
        { code: 30, name: "Tỉnh Hải Dương", type: "Tỉnh", shortName: "Hải Dương" },
        { code: 93, name: "Tỉnh Hậu Giang", type: "Tỉnh", shortName: "Hậu Giang" },
        { code: 17, name: "Tỉnh Hòa Bình", type: "Tỉnh", shortName: "Hòa Bình", hotSpots: ["Mai Châu"] },
        { code: 33, name: "Tỉnh Hưng Yên", type: "Tỉnh", shortName: "Hưng Yên" },
        { code: 62, name: "Tỉnh Kon Tum", type: "Tỉnh", shortName: "Kon Tum", hotSpots: ["Măng Đen"] },
        { code: 12, name: "Tỉnh Lai Châu", type: "Tỉnh", shortName: "Lai Châu" },
        { code: 20, name: "Tỉnh Lạng Sơn", type: "Tỉnh", shortName: "Lạng Sơn" },
        { code: 80, name: "Tỉnh Long An", type: "Tỉnh", shortName: "Long An" },
        { code: 36, name: "Tỉnh Nam Định", type: "Tỉnh", shortName: "Nam Định" },
        { code: 40, name: "Tỉnh Nghệ An", type: "Tỉnh", shortName: "Nghệ An", hotSpots: ["Cửa Lò"] },
        { code: 58, name: "Tỉnh Ninh Thuận", type: "Tỉnh", shortName: "Ninh Thuận", hotSpots: ["Phan Rang"] },
        { code: 25, name: "Tỉnh Phú Thọ", type: "Tỉnh", shortName: "Phú Thọ" },
        { code: 44, name: "Tỉnh Quảng Bình", type: "Tỉnh", shortName: "Quảng Bình", hotSpots: ["Phong Nha", "Đồng Hới"] },
        { code: 51, name: "Tỉnh Quảng Ngãi", type: "Tỉnh", shortName: "Quảng Ngãi", hotSpots: ["Lý Sơn"] },
        { code: 45, name: "Tỉnh Quảng Trị", type: "Tỉnh", shortName: "Quảng Trị" },
        { code: 94, name: "Tỉnh Sóc Trăng", type: "Tỉnh", shortName: "Sóc Trăng" },
        { code: 14, name: "Tỉnh Sơn La", type: "Tỉnh", shortName: "Sơn La", hotSpots: ["Mộc Châu"] },
        { code: 72, name: "Tỉnh Tây Ninh", type: "Tỉnh", shortName: "Tây Ninh" },
        { code: 34, name: "Tỉnh Thái Bình", type: "Tỉnh", shortName: "Thái Bình" },
        { code: 19, name: "Tỉnh Thái Nguyên", type: "Tỉnh", shortName: "Thái Nguyên" },
        { code: 38, name: "Tỉnh Thanh Hóa", type: "Tỉnh", shortName: "Thanh Hóa", hotSpots: ["Sầm Sơn"] },
        { code: 82, name: "Tỉnh Tiền Giang", type: "Tỉnh", shortName: "Tiền Giang" },
        { code: 84, name: "Tỉnh Trà Vinh", type: "Tỉnh", shortName: "Trà Vinh" },
        { code: 8, name: "Tỉnh Tuyên Quang", type: "Tỉnh", shortName: "Tuyên Quang" },
        { code: 86, name: "Tỉnh Vĩnh Long", type: "Tỉnh", shortName: "Vĩnh Long" },
        { code: 26, name: "Tỉnh Vĩnh Phúc", type: "Tỉnh", shortName: "Vĩnh Phúc", hotSpots: ["Tam Đảo"] },
        { code: 15, name: "Tỉnh Yên Bái", type: "Tỉnh", shortName: "Yên Bái", hotSpots: ["Mù Cang Chải"] }
    ];

    const POPULAR_DESTINATIONS = [
        { name: "Đà Lạt", province: "Lâm Đồng", desc: "Thành phố ngàn hoa, khí hậu mát mẻ quanh năm", icon: "bi-flower1" },
        { name: "Hội An", province: "Đà Nẵng", desc: "Phố cổ di sản, ánh đèn lồng rực rỡ", icon: "bi-lamp" },
        { name: "Đà Nẵng", province: "Đà Nẵng", desc: "Thành phố biển hiện đại, cầu Rồng, bán đảo Sơn Trà", icon: "bi-water" },
        { name: "Nha Trang", province: "Khánh Hòa", desc: "Vịnh biển tuyệt đẹp, bãi cát trắng mịn màng", icon: "bi-brightness-high" },
        { name: "Vũng Tàu", province: "Hồ Chí Minh", desc: "Nghỉ dưỡng biển ven thành phố", icon: "bi-compass" },
        { name: "Phú Quốc", province: "Kiên Giang", desc: "Đảo ngọc thiên đường, hoàng hôn lãng mạn", icon: "bi-sun-fill" },
        { name: "Sa Pa", province: "Lào Cai", desc: "Thị trấn trong sương, ruộng bậc thang kỳ vĩ", icon: "bi-cloud-sun" },
        { name: "Hạ Long", province: "Quảng Ninh", desc: "Kỳ quan thiên nhiên thế giới, du thuyền vịnh xanh", icon: "bi-geo" },
        { name: "Hà Nội", province: "Hà Nội", desc: "Thủ đô ngàn năm văn hiến, phố cổ trầm mặc", icon: "bi-bank" },
        { name: "Hồ Chí Minh", province: "Hồ Chí Minh", desc: "Đô thị sôi động, ẩm thực đường phố phong phú", icon: "bi-building" },
        { name: "Huế", province: "Huế", desc: "Cố đô thơ mộng, đền đài lăng tẩm cổ kính", icon: "bi-gem" },
        { name: "Ninh Bình", province: "Ninh Bình", desc: "Non nước hữu tình, Tràng An - Tam Cốc", icon: "bi-tree" },
        { name: "Quy Nhơn", province: "Bờ biển miền Trung", desc: "Biển xanh trong vắt, Kỳ Co - Eo Gió hoang sơ", icon: "bi-umbrella" },
        { name: "Phan Thiết", province: "Nam Trung Bộ", desc: "Đồi cát bay, Mũi Né resort ven biển", icon: "bi-tsunami" }
    ];

    let cachedProvinces = null;
    const cachedDistricts = new Map();

    function removeVietnameseTones(str) {
        if (!str) return '';
        str = str.toLowerCase();
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
        str = str.replace(/đ/g, "d");
        str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, "");
        str = str.replace(/\u02C6|\u031B|\u0306/g, "");
        return str.trim();
    }

    async function getProvinces() {
        if (cachedProvinces && cachedProvinces.length > 0) return cachedProvinces;

        try {
            const res = await fetch('https://esgoo.net/api-tinhthanh-new/1/0.htm', { cache: 'force-cache' });
            if (res.ok) {
                const apiData = await res.json();
                if (apiData && apiData.error === 0 && Array.isArray(apiData.data)) {
                    cachedProvinces = apiData.data.map(p => {
                        const local = VIETNAM_PROVINCES.find(lp => removeVietnameseTones(lp.name).includes(removeVietnameseTones(p.name)));
                        return {
                            code: p.id,
                            name: p.full_name,
                            shortName: p.name,
                            hotSpots: local?.hotSpots || []
                        };
                    });
                    return cachedProvinces;
                }
            }
        } catch (e) {
            console.error("Failed to fetch Vietnam provinces API (new):", e);
        }

        // Không dùng fallback VIETNAM_PROVINCES để đảm bảo chỉ dùng dữ liệu sau sáp nhập.
        return [];
    }

    async function getDistrictsAndWards(provinceCodeOrName) {
        let province = null;
        const provinces = await getProvinces();

        if (typeof provinceCodeOrName === 'number' || !isNaN(Number(provinceCodeOrName))) {
            province = provinces.find(p => p.code == provinceCodeOrName);
        } else if (typeof provinceCodeOrName === 'string') {
            const norm = removeVietnameseTones(provinceCodeOrName);
            province = provinces.find(p => {
                const pNorm = removeVietnameseTones(p.name);
                const sNorm = removeVietnameseTones(p.shortName || '');
                return pNorm === norm || sNorm === norm || pNorm.includes(norm) || norm.includes(pNorm) || sNorm.includes(norm) || norm.includes(sNorm);
            });
        }

        if (!province) return [];

        if (cachedDistricts.has(province.code)) {
            return cachedDistricts.get(province.code);
        }

        try {
            // New structure: 2 levels (Province -> Ward/Commune), so we fetch wards directly for the province.
            const wRes = await fetch(`https://esgoo.net/api-tinhthanh-new/2/${province.code}.htm`, { cache: 'force-cache' });
            if (wRes.ok) {
                const wData = await wRes.json();
                if (wData.error === 0 && Array.isArray(wData.data)) {
                    const wards = wData.data.map(w => ({
                        name: w.name,
                        district: "", // District level is skipped in new boundary structure
                        fullName: w.full_name
                    }));
                    cachedDistricts.set(province.code, wards);
                    return wards;
                }
            }
        } catch (e) {
            // ignore
        }

        return [];
    }

    /**
     * Initializes Autocomplete for Destination Search in Hero Floating Bar
     */
    function initDestinationAutocomplete(inputEl, dropdownEl, onSelect) {
        if (!inputEl || !dropdownEl) return;

        let activeIndex = -1;
        let currentItems = [];

        function renderList(query) {
            dropdownEl.innerHTML = '';
            activeIndex = -1;
            const cleanQuery = removeVietnameseTones(query);

            if (!cleanQuery) {
                // Show Top Popular Destinations
                const title = document.createElement('div');
                title.className = 'destination-autocomplete-section-title';
                title.innerHTML = '<i class="bi bi-fire text-danger me-1"></i> Điểm đến thịnh hành';
                dropdownEl.appendChild(title);

                currentItems = POPULAR_DESTINATIONS;
                currentItems.forEach((dest, idx) => {
                    const item = document.createElement('div');
                    item.className = 'destination-item';
                    item.dataset.index = idx;
                    item.innerHTML = `
                        <div class="destination-item-icon">
                            <i class="bi ${dest.icon || 'bi-geo-alt-fill'}"></i>
                        </div>
                        <div class="destination-item-info">
                            <div class="destination-item-name">${dest.name}</div>
                            <div class="destination-item-sub">${dest.desc || dest.province}</div>
                        </div>
                    `;
                    item.addEventListener('click', () => selectItem(dest.name));
                    dropdownEl.appendChild(item);
                });
            } else {
                // Search across 63 provinces + popular destinations
                const results = [];

                POPULAR_DESTINATIONS.forEach(d => {
                    const dNorm = removeVietnameseTones(d.name);
                    const pNorm = removeVietnameseTones(d.province);
                    if (dNorm.includes(cleanQuery) || pNorm.includes(cleanQuery)) {
                        results.push({
                            name: d.name,
                            sub: `Điểm du lịch nổi tiếng • ${d.province}`,
                            icon: d.icon || 'bi-geo-alt-fill'
                        });
                    }
                });

                VIETNAM_PROVINCES.forEach(p => {
                    const nameNorm = removeVietnameseTones(p.name);
                    const shortNorm = removeVietnameseTones(p.shortName);
                    if (nameNorm.includes(cleanQuery) || shortNorm.includes(cleanQuery)) {
                        if (!results.some(r => r.name.toLowerCase() === p.shortName.toLowerCase())) {
                            results.push({
                                name: p.shortName,
                                sub: p.name,
                                icon: 'bi-building'
                            });
                        }
                    }
                });

                if (results.length === 0) {
                    dropdownEl.innerHTML = `
                        <div class="p-3 text-center text-muted small">
                            <i class="bi bi-geo-alt text-muted fs-4 d-block mb-1"></i>
                            Không tìm thấy điểm đến khớp với "<strong>${escapeHtml(query)}</strong>".<br>
                            Bạn có thể nhấn Tìm kiếm để xem toàn bộ homestay.
                        </div>
                    `;
                    currentItems = [];
                    return;
                }

                currentItems = results.slice(0, 8);
                const title = document.createElement('div');
                title.className = 'destination-autocomplete-section-title';
                title.innerHTML = `<i class="bi bi-search me-1"></i> Gợi ý điểm đến (${results.length})`;
                dropdownEl.appendChild(title);

                currentItems.forEach((dest, idx) => {
                    const item = document.createElement('div');
                    item.className = 'destination-item';
                    item.dataset.index = idx;
                    item.innerHTML = `
                        <div class="destination-item-icon">
                            <i class="bi ${dest.icon}"></i>
                        </div>
                        <div class="destination-item-info">
                            <div class="destination-item-name">${dest.name}</div>
                            <div class="destination-item-sub">${dest.sub}</div>
                        </div>
                    `;
                    item.addEventListener('click', () => selectItem(dest.name));
                    dropdownEl.appendChild(item);
                });
            }

            dropdownEl.classList.remove('d-none');
        }

        function selectItem(name) {
            inputEl.value = name;
            dropdownEl.classList.add('d-none');
            if (typeof onSelect === 'function') {
                onSelect(name);
            }
        }

        inputEl.addEventListener('focus', () => {
            renderList(inputEl.value);
        });

        inputEl.addEventListener('input', () => {
            renderList(inputEl.value);
        });

        inputEl.addEventListener('keydown', (e) => {
            if (dropdownEl.classList.contains('d-none')) {
                if (e.key === 'ArrowDown') {
                    renderList(inputEl.value);
                    e.preventDefault();
                }
                return;
            }

            const items = dropdownEl.querySelectorAll('.destination-item');
            if (!items.length) return;

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                activeIndex = (activeIndex + 1) % items.length;
                updateActive(items);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                activeIndex = (activeIndex - 1 + items.length) % items.length;
                updateActive(items);
            } else if (e.key === 'Enter') {
                if (activeIndex >= 0 && activeIndex < currentItems.length) {
                    e.preventDefault();
                    selectItem(currentItems[activeIndex].name);
                }
            } else if (e.key === 'Escape') {
                dropdownEl.classList.add('d-none');
            }
        });

        function updateActive(items) {
            items.forEach((el, idx) => {
                el.classList.toggle('active', idx === activeIndex);
                if (idx === activeIndex) {
                    el.scrollIntoView({ block: 'nearest' });
                }
            });
        }

        document.addEventListener('click', (e) => {
            if (!inputEl.contains(e.target) && !dropdownEl.contains(e.target)) {
                dropdownEl.classList.add('d-none');
            }
        });
    }

    /**
     * Helper to attach custom autocomplete dropdown to an input
     */
    function attachCustomDropdown(inputEl, getItemsFn) {
        if (!inputEl) return;
        
        let dropdownEl = document.createElement('div');
        dropdownEl.className = 'destination-autocomplete-dropdown d-none';
        dropdownEl.style.width = '100%';
        dropdownEl.style.top = 'calc(100% + 4px)';
        dropdownEl.style.zIndex = '1060';
        
        const parent = inputEl.parentElement;
        parent.classList.add('position-relative');
        parent.appendChild(dropdownEl);

        let activeIndex = -1;
        let currentItems = [];

        async function renderList(query) {
            dropdownEl.innerHTML = '';
            activeIndex = -1;
            const items = await getItemsFn(query);
            currentItems = items;

            if (items.length === 0) {
                const empty = document.createElement('div');
                empty.className = 'p-3 text-muted small text-center';
                empty.textContent = 'Không tìm thấy kết quả phù hợp';
                dropdownEl.appendChild(empty);
            } else {
                items.slice(0, 100).forEach((item, idx) => {
                    const div = document.createElement('div');
                    div.className = 'destination-item';
                    div.dataset.index = idx;
                    div.innerHTML = `
                        <div class="destination-item-icon">
                            <i class="bi bi-geo-alt-fill text-primary"></i>
                        </div>
                        <div class="destination-item-info">
                            <div class="destination-item-name">${item.name}</div>
                            ${item.sub ? `<div class="destination-item-sub">${item.sub}</div>` : ''}
                        </div>
                    `;
                    div.addEventListener('click', () => {
                        inputEl.value = item.value;
                        dropdownEl.classList.add('d-none');
                        inputEl.dispatchEvent(new Event('change'));
                    });
                    dropdownEl.appendChild(div);
                });
            }
            dropdownEl.classList.remove('d-none');
        }

        inputEl.addEventListener('focus', () => renderList(inputEl.value));
        
        // Debounce input to avoid spamming
        let timeout;
        inputEl.addEventListener('input', () => {
            clearTimeout(timeout);
            timeout = setTimeout(() => renderList(inputEl.value), 200);
        });

        inputEl.addEventListener('keydown', (e) => {
            const domItems = dropdownEl.querySelectorAll('.destination-item');
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                if (dropdownEl.classList.contains('d-none')) {
                    renderList(inputEl.value);
                } else if (domItems.length > 0) {
                    activeIndex = (activeIndex + 1) % domItems.length;
                    updateActive(domItems);
                }
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                if (!dropdownEl.classList.contains('d-none') && domItems.length > 0) {
                    activeIndex = (activeIndex - 1 + domItems.length) % domItems.length;
                    updateActive(domItems);
                }
            } else if (e.key === 'Enter') {
                if (!dropdownEl.classList.contains('d-none') && activeIndex >= 0 && activeIndex < currentItems.length) {
                    e.preventDefault();
                    inputEl.value = currentItems[activeIndex].value;
                    dropdownEl.classList.add('d-none');
                    inputEl.dispatchEvent(new Event('change'));
                }
            } else if (e.key === 'Escape') {
                dropdownEl.classList.add('d-none');
            }
        });

        function updateActive(domItems) {
            domItems.forEach((el, idx) => {
                el.classList.toggle('active', idx === activeIndex);
                if (idx === activeIndex) {
                    el.scrollIntoView({ block: 'nearest' });
                }
            });
        }

        document.addEventListener('click', (e) => {
            if (!inputEl.contains(e.target) && !dropdownEl.contains(e.target)) {
                dropdownEl.classList.add('d-none');
            }
        });
    }

    /**
     * Initializes Reverse Order City & Ward Autocomplete for Property Creation Forms
     */
    function initPropertyAddressAutocomplete(cityInputEl, wardInputEl, addressInputEl) {
        if (!cityInputEl) return;

        cityInputEl.setAttribute('autocomplete', 'off');
        
        attachCustomDropdown(cityInputEl, async (query) => {
            const provinces = await getProvinces();
            const cleanQuery = removeVietnameseTones(query);
            return provinces.filter(p => {
                if (!cleanQuery) return true;
                const nameNorm = removeVietnameseTones(p.name);
                const shortNorm = removeVietnameseTones(p.shortName);
                return nameNorm.includes(cleanQuery) || shortNorm.includes(cleanQuery);
            }).map(p => ({
                name: p.shortName || p.name,
                value: p.shortName || p.name,
                sub: p.name
            }));
        });

        // 2. Setup Ward Custom Dropdown
        if (wardInputEl) {
            wardInputEl.setAttribute('autocomplete', 'off');

            function disableDependentInputs() {
                if (wardInputEl) {
                    wardInputEl.disabled = true;
                    wardInputEl.placeholder = "Vui lòng chọn Thành phố trước";
                }
                if (addressInputEl) {
                    addressInputEl.disabled = true;
                    addressInputEl.placeholder = "Vui lòng chọn Thành phố trước";
                }
            }

            function enableDependentInputs() {
                if (wardInputEl) {
                    wardInputEl.disabled = false;
                    wardInputEl.placeholder = "Chọn hoặc nhập Phường / Xã";
                }
                if (addressInputEl) {
                    addressInputEl.disabled = false;
                    addressInputEl.placeholder = "Ví dụ: 14/2 Khởi Nghĩa Bắc Sơn";
                }
            }

            // Initially disable them if city is empty
            if (!cityInputEl.value) {
                disableDependentInputs();
            } else {
                // Background check if valid
                getDistrictsAndWards(cityInputEl.value).then(w => {
                    if (w && w.length > 0) enableDependentInputs();
                });
            }

            attachCustomDropdown(wardInputEl, async (query) => {
                if (!cityInputEl.value) return [];
                const wards = await getDistrictsAndWards(cityInputEl.value);
                const cleanQuery = removeVietnameseTones(query);
                return wards.filter(w => {
                    if (!cleanQuery) return true;
                    const wNorm = removeVietnameseTones(w.fullName);
                    return wNorm.includes(cleanQuery);
                }).map(w => ({
                    name: w.name,
                    value: w.fullName,
                    sub: w.district
                }));
            });

            cityInputEl.addEventListener('change', async () => {
                const val = cityInputEl.value.trim();
                if (val !== "") {
                    if (wardInputEl && !wardInputEl.disabled) wardInputEl.value = "";
                    if (addressInputEl && !addressInputEl.disabled) addressInputEl.value = "";
                    const wards = await getDistrictsAndWards(val);
                    if (wards && wards.length > 0) {
                        enableDependentInputs();
                    } else {
                        disableDependentInputs();
                    }
                } else {
                    disableDependentInputs();
                    if (wardInputEl) wardInputEl.value = "";
                    if (addressInputEl) addressInputEl.value = "";
                }
            });

            cityInputEl.addEventListener('input', () => {
                if (!cityInputEl.value.trim()) {
                    disableDependentInputs();
                    if (wardInputEl) wardInputEl.value = "";
                    if (addressInputEl) addressInputEl.value = "";
                }
            });
        }
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // Export to window
    window.StaylyAddress = {
        getProvinces,
        getDistrictsAndWards,
        initDestinationAutocomplete,
        initPropertyAddressAutocomplete,
        POPULAR_DESTINATIONS,
        VIETNAM_PROVINCES
    };
})(window);
