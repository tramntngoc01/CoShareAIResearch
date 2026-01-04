
# Sample Data (Fake, Non-Production)

Keep all sample data non-sensitive. All names, emails, phones and IDs below are synthetic and for dev/test only.

---

## 1. USERS & AUTH

### 1.1 End Users (Tầng 1/2/3)

| userId | fullName          | tier | phone       | companyId | companyName                  | pickupPointId | pickupPointName                   | refTierCode | status   |
|--------|-------------------|------|------------|-----------|------------------------------|---------------|------------------------------------|------------|----------|
| 10001  | Trần Thị B        | 3    | 0912000001 | 201       | Công ty May KCN A            | 301           | Điểm nhận KCN A - Cổng chính      | T2_001     | Active   |
| 10002  | Nguyễn Văn C      | 3    | 0912000002 | 201       | Công ty May KCN A            | 301           | Điểm nhận KCN A - Cổng chính      | T2_001     | Active   |
| 10003  | Lê Thị D          | 3    | 0912000003 | 202       | Công ty Điện tử KCN B        | 302           | Điểm nhận KCN B - Nhà xe          | T2_002     | Active   |
| 11001  | Phạm Văn Ref1     | 2    | 0903000001 | 201       | Công ty May KCN A            | 301           | Điểm nhận KCN A - Cổng chính      | T1_001     | Active   |
| 11002  | Đỗ Thị Ref2       | 2    | 0903000002 | 202       | Công ty Điện tử KCN B        | 302           | Điểm nhận KCN B - Nhà xe          | T1_001     | Active   |
| 12001  | Nguyễn Quốc Leader| 1    | 0904000001 | null      | (Mạng lưới toàn KCN)         | null          | -                                  | null       | Active   |

Notes:
- Tầng 1 dùng code `T1_001`, Tầng 2 dùng `T2_001`/`T2_002` làm refTier cho Tầng 3 (phù hợp BR-USERS-002).
- Phones are fake local-format numbers.

### 1.2 Admin Users (Admin Portal)

| adminUserId | fullName              | loginId                  | type   | roles                        | companyScope        | status  |
|-------------|-----------------------|--------------------------|--------|------------------------------|---------------------|---------|
| 9001        | Trần Minh SuperAdmin  | super.admin@example.com  | Email  | ["SuperAdmin"]              | All                 | Active  |
| 9002        | Lê Hồng Ops           | ops.admin@example.com    | Email  | ["Ops"]                     | All                 | Active  |
| 9003        | Phạm Lan Finance      | finance.admin@example.com| Email  | ["Finance"]                 | All                 | Active  |
| 9004        | Nguyễn Hải QC         | qc.admin@example.com     | Email  | ["QC"]                      | All                 | Active  |
| 9005        | Vũ Thu Support        | support.admin@example.com| Email  | ["Support"]                 | All                 | Active  |

### 1.3 Sample AUTH tokens / sessions (conceptual)

| userId | deviceLabel        | sessionToken         | refreshToken          | issuedAt                 | expiresAt                | revoked |
|--------|--------------------|----------------------|-----------------------|--------------------------|--------------------------|---------|
| 10001  | Android_KCNA_Phone | sess_10001_dev1      | ref_10001_dev1        | 2026-01-05T01:00:00Z     | 2026-01-05T03:00:00Z     | false   |
| 10002  | Web_Office_PC      | sess_10002_web       | ref_10002_web         | 2026-01-05T02:00:00Z     | 2026-01-05T04:00:00Z     | false   |

---

## 2. ADMIN & COMPANY / PICKUP POINT

### 2.1 Companies

| companyId | companyCode | companyName             | kcn       | payLaterEnabled | creditLimitPolicy                      | status |
|-----------|-------------|-------------------------|-----------|-----------------|----------------------------------------|--------|
| 201       | CTY_MAY_A   | Công ty May KCN A       | KCN A     | true            | Tối đa 3 đơn, tổng 5.000.000 / User   | Active |
| 202       | CTY_DT_B    | Công ty Điện tử KCN B   | KCN B     | false           | N/A                                    | Active |

### 2.2 Pickup Points

| pickupPointId | companyId | name                                | address                       | isDefault | status |
|---------------|-----------|-------------------------------------|-------------------------------|-----------|--------|
| 301           | 201       | Điểm nhận KCN A - Cổng chính       | Cổng chính Công ty May KCN A | true      | Active |
| 302           | 202       | Điểm nhận KCN B - Nhà xe           | Nhà xe Công ty Điện tử KCN B | true      | Active |

---

## 3. CATALOG (Products, Categories, Lots)

### 3.1 Categories

| categoryId | categoryCode | categoryName     |
|-----------:|--------------|------------------|
| 10         | FOOD_DRY     | Thực phẩm khô    |
| 11         | HOME_KITCHEN | Đồ gia dụng      |

### 3.2 Lots (QC-controlled)

| lotId | lotCode   | categoryId | supplierName      | qcStatus      | qcApprovedAt            |
|-------|-----------|-----------:|-------------------|---------------|-------------------------|
| 1001  | LOT_FD_01 | 10         | NCC Thực phẩm An | Approved      | 2025-12-20T08:00:00Z    |
| 1002  | LOT_HK_01 | 11         | NCC Gia dụng Bền | Approved      | 2025-12-22T09:30:00Z    |

### 3.3 Products

| productId | productCode | name                          | categoryId | lotId | unit      | unitPrice | isActive |
|-----------|-------------|-------------------------------|-----------:|------:|-----------|----------:|---------:|
| 2001      | PROD_FD_01  | Mì gói CoShare vị gà (thùng) | 10         | 1001  | thùng 30gói | 120000   | 1        |
| 2002      | PROD_FD_02  | Sữa hộp CoShare 180ml        | 10         | 1001  | lốc 4 hộp | 90000    | 1        |
| 2003      | PROD_HK_01  | Ấm siêu tốc CoShare 1.8L     | 11         | 1002  | cái       | 250000   | 1        |

### 3.4 Inventory snapshot

| productId | warehouse    | quantity |
|-----------|--------------|---------:|
| 2001      | KHO_KCN_A    | 300      |
| 2002      | KHO_KCN_A    | 500      |
| 2003      | KHO_KCN_A    | 80       |

---

## 4. ORDERS & ORDER ITEMS

### 4.1 Orders (mix of COD and pay-later)

| orderId | orderCode | userId | companyId | pickupPointId | createdAt               | status         | paymentMethod | totalAmount |
|---------|-----------|--------|-----------|---------------|-------------------------|----------------|---------------|------------:|
| 70001   | ORD-70001 | 10001  | 201       | 301           | 2026-01-05T01:15:00Z    | Đã nhận        | COD           | 435000      |
| 70002   | ORD-70002 | 10002  | 201       | 301           | 2026-01-05T02:00:00Z    | Sẵn sàng nhận  | COD           | 310000      |
| 70003   | ORD-70003 | 10003  | 202       | 302           | 2026-01-04T23:30:00Z    | Đang giao      | TRANSFER      | 250000      |
| 70004   | ORD-70004 | 10001  | 201       | 301           | 2026-01-03T20:00:00Z    | Đã nhận        | CREDIT        | 400000      |

### 4.2 Order Items

| orderId | productId | quantity | unitPrice | lineAmount |
|--------:|-----------|---------:|----------:|-----------:|
| 70001   | 2001      | 2        | 120000    | 240000     |
| 70001   | 2002      | 3        | 65000     | 195000     |
| 70002   | 2003      | 1        | 250000    | 250000     |
| 70002   | 2002      | 1        | 60000     | 60000      |
| 70003   | 2003      | 1        | 250000    | 250000     |
| 70004   | 2002      | 4        | 100000    | 400000     |

---

## 5. PAYMENTS, RECEIPTS, SHIFTS, DEBT

### 5.1 Payments per order (conceptual)

| paymentId | orderId | paymentMethod | amount  | status        |
|-----------|--------:|---------------|--------:|---------------|
| 80001     | 70001   | COD           | 435000  | PAID          |
| 80002     | 70002   | COD           | 310000  | UNPAID        |
| 80003     | 70003   | TRANSFER      | 250000  | PENDING       |
| 80004     | 70004   | CREDIT        | 400000  | CREDIT_DUE    |

### 5.2 COD Receipts

| receiptId | orderId | amount | collectedBy | collectedAt              | shiftId |
|-----------|--------:|-------:|-------------|--------------------------|--------:|
| 90001     | 70001   | 435000 | shipper_200 | 2026-01-05T03:05:00Z     | 30001   |

### 5.3 Shifts & Reconciliation

| shiftId | shipperId   | startTime              | endTime                | expectedCod | collectedCod | difference | status      |
|--------:|-------------|------------------------|------------------------|------------:|------------:|-----------:|------------|
| 30001   | shipper_200 | 2026-01-05T00:00:00Z   | 2026-01-05T06:00:00Z   | 450000      | 435000      | -15000     | DIFFERENCE  |

### 5.4 Credit limit & debt balances (pay-later)

| creditLimitId | userId | companyId | limitAmount | currentDebt | asOfDate    |
|--------------:|--------|----------:|------------:|------------:|------------|
| 50001         | 10001  | 201       | 2000000     | 400000      | 2026-01-05 |

---

## 6. NOTIFICATIONS (Templates & Logs)

### 6.1 ZNS Templates

| templateCode       | type                 | description                                   | active |
|--------------------|----------------------|-----------------------------------------------|--------|
| OTP_REGISTER_V1    | OTP                  | OTP đăng ký/đăng nhập End User                | 1      |
| REMIND_PICKUP_V1   | PickupReminder       | Nhắc nhận hàng tại điểm nhận                  | 1      |
| REMIND_DEBT_V1     | DebtReminder         | Nhắc công nợ trả chậm sắp đến hạn/quá hạn     | 1      |
| COMM_CHANGE_V1     | CommissionChange     | Thông báo thay đổi chính sách hoa hồng        | 0      |

### 6.2 Notification logs (simplified)

| notificationId | templateCode    | channel | recipientPhone | eventRef      | status   | sentAt                  |
|----------------|-----------------|---------|----------------|--------------|----------|-------------------------|
| NTF-0001       | OTP_REGISTER_V1 | ZNS     | 0912000001     | AUTH-SIGNUP  | SENT     | 2026-01-05T01:16:00Z    |
| NTF-0002       | REMIND_PICKUP_V1| ZNS     | 0912000002     | ORD-70002    | SENT     | 2026-01-05T09:00:00Z    |
| NTF-0003       | REMIND_DEBT_V1  | ZNS     | 0912000001     | DEBT-50001   | QUEUED   | 2026-01-06T08:00:00Z    |

---

## 7. REPORTING (Time ranges & expected aggregates)

### 7.1 Reporting periods

- Sample day: 2026-01-05  
- Sample week: 2026-01-01 → 2026-01-07  
- Sample month: 2026-01 (01/01/2026–31/01/2026)

### 7.2 Example sales summary (for Company 201, 2026-01-01 → 2026-01-07)

| periodLabel | companyId | totalOrders | totalRevenue | totalItems |
|-------------|----------:|-----------:|-------------:|-----------:|
| 2026-01-01→07 | 201     | 3          | 1_145_000    | 11        |

### 7.3 Example commission snapshot (Tier 2)

| affiliateId | affiliateName  | period   | categoryName   | totalRevenue | commissionRate | commissionAmount |
|-------------|----------------|---------|----------------|-------------:|---------------:|-----------------:|
| T2_001      | Phạm Văn Ref1  | 2026-01 | Thực phẩm khô  | 800000       | 0.05           | 40000            |

### 7.4 Example debt report (Company 201)

| companyId | companyName           | asOfDate   | totalDebt | inTermDebt | overdueDebt |
|----------:|----------------------|-----------|----------:|-----------:|-----------:|
| 201       | Công ty May KCN A    | 2026-01-05| 400000    | 300000     | 100000     |

---

## 8. Cross-module scenarios (sanity for E2E tests)

- Scenario A — End User `10001` (Tầng 3, Công ty May KCN A):
	- Registers via AUTH using phone 0912000001 → NOTIFICATIONS sends OTP_REGISTER_V1.
	- Places COD order `70001` with products 2001 and 2002 at pickup point 301.
	- Shipper `shipper_200` collects COD, creates receipt `90001`, closes shift `30001` with small difference.
	- REPORTING shows this order in sales summary and COD by shift, PAYMENTS reflects PAID COD.

- Scenario B — Pay-later order for the same user:
	- User `10001` places credit order `70004` (CREDIT) within credit limit `50001`.
	- PAYMENTS updates currentDebt to 400000, REPORTING shows the debt in company 201’s debt report.

