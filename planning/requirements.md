# Requirements Document

## Introduction

The Online Healthcare Platform is a comprehensive digital health ecosystem that connects patients, private doctors, medical insurance providers, and pharmacies in a unified, accessible experience.

**Deployment context:** The first production pilot targets **Zimbabwe**, with **USD** as the primary billing currency for MVP, **configurable** payment and SMS providers, and doctor verification aligned with **Health Professions Council of Zimbabwe (HPCZ)**-style licensing identifiers (exact rules require local legal and clinical governance). Requirements apply to the **full** platform; **[mvp.md](./mvp.md)** lists which requirements are **deferred** for the initial release. The platform enables telemedicine consultations via video, audio, and chat; GPS-based physical appointment booking; e-prescriptions; medication ordering and delivery; medication adherence tracking; emergency contact management; and pharmacy workflow management. Future phases will introduce IoT-based patient monitoring for chronic conditions (NCDs) and full emergency response integration.

The platform is designed to be innovative, inclusive, and patient-centric — reducing barriers to quality healthcare access while empowering doctors, pharmacies, and insurers with modern digital tooling.

---

## Glossary

- **Patient**: A registered user seeking healthcare services through the platform.
- **Doctor**: A licensed private medical practitioner registered on the platform.
- **Pharmacy**: A registered pharmacy entity that fulfills prescriptions and delivers medication.
- **Insurer**: A medical insurance provider integrated with the platform for claims and payment.
- **Next_of_Kin**: An emergency contact designated by a Patient.
- **Prescription**: A digital document issued by a Doctor authorizing a Patient to obtain specific medication.
- **Appointment**: A scheduled consultation between a Patient and a Doctor, either virtual or physical.
- **Telemedicine_Session**: A real-time virtual consultation conducted via video, audio, or chat.
- **Medication_Schedule**: A Patient's personalized plan for taking prescribed medication at defined intervals.
- **Adherence_Event**: A recorded instance of a Patient confirming or missing a medication dose.
- **GPS_Location**: The geographic coordinates of a Patient or Doctor used for proximity-based matching.
- **IoT_Device**: A connected health monitoring device that transmits biometric data to the platform.
- **NCD**: Non-Communicable Disease (e.g., hypertension, diabetes).
- **AI_Assistant**: An intelligent system component that provides health insights, triage, and recommendations.
- **Health_Record**: A Patient's longitudinal medical history stored on the platform.
- **Notification_Service**: The platform component responsible for delivering alerts via push, SMS, and email.
- **Payment_Gateway**: The component handling local payment methods, insurance claims, and credit transactions.
- **Rating**: A numerical score (1–5) given by a Patient to a Doctor after a completed consultation.
- **Availability_Slot**: A time window during which a Doctor is available for appointments.
- **Delivery_Agent**: A person or service responsible for delivering medication from a Pharmacy to a Patient.
- **Credit_Line**: A revolving credit facility extended to a Patient by the platform for healthcare expenses.
- **Credit_Score**: A numerical rating computed from a Patient's payment history on the platform, used to determine Credit_Line eligibility and limit.
- **Instalment_Plan**: A structured repayment schedule that divides a large healthcare expense into smaller periodic payments.
- **Lab_Test**: A diagnostic examination ordered by a Doctor, including blood work, urinalysis, imaging, and other pathology tests.
- **Lab_Result**: The digital output of a completed Lab_Test, including reports, images, and pathology findings.
- **Radiology_Report**: A structured report produced by a radiologist describing findings from imaging studies (X-ray, MRI, CT scan, ultrasound).
- **Lab_Partner**: A registered diagnostic laboratory or imaging centre integrated with the platform.
- **Therapy_Session**: A mental health consultation conducted between a Patient and a licensed therapist via the platform.
- **Mood_Log**: A Patient-recorded entry capturing emotional state and associated notes at a point in time.
- **Crisis_Protocol**: A defined escalation procedure triggered when a Patient indicates a mental health emergency.
- **Dental_Consultation**: A specialist consultation for oral health conducted through the platform.
- **Optical_Consultation**: A specialist consultation for vision and eye health conducted through the platform.
- **Optical_Prescription**: A Doctor-issued document specifying corrective lens parameters for a Patient.
- **Antenatal_Record**: A structured health record tracking pregnancy progress, including gestational age, fetal measurements, and maternal vitals.
- **Birth_Plan**: A Patient-authored document outlining preferences for labour, delivery, and postnatal care.
- **Child_Profile**: A dependent health profile linked to a parent or guardian's Patient account.
- **Guardian**: A parent or legally designated caregiver who manages a Child_Profile on the platform.
- **Vaccination_Record**: A log of administered vaccines associated with a Patient or Child_Profile.
- **Growth_Entry**: A recorded measurement of a child's height, weight, or developmental milestone at a point in time.
- **Referral**: A formal request issued by a Doctor directing a Patient to another Doctor, specialist, or hospital for further care.
- **Referral_Status**: The current state of a Referral (e.g., pending, accepted, completed, declined).
- **Health_Goal**: A Patient-defined target for a measurable health metric (e.g., daily steps, weight, sleep duration, water intake).
- **Care_Plan**: A structured program of interventions and monitoring tasks assigned to a Patient for managing a chronic condition.
- **Second_Opinion**: A clinical review of a Patient's Health_Record by a Doctor who is not the Patient's primary treating physician.
- **Admin**: A platform operator with elevated privileges for user management, verification, and analytics.
- **Queue_Entry**: A record representing a Patient's position in a physical clinic's virtual waiting queue.
- **Estimated_Wait_Time**: A dynamically computed duration indicating how long a Patient is expected to wait before being seen at a physical clinic.

---

## Requirements

### Requirement 1: Patient Registration and Profile Management

**User Story:** As a patient, I want to register and manage my health profile, so that doctors and the platform can provide personalized care.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to register using a phone number, email address, or social login (Google/Apple).
2. WHEN a Patient completes registration, THE Platform SHALL create a Health_Record linked to that Patient's account.
3. THE Patient SHALL be able to update personal details including name, date of birth, blood type, known allergies, and chronic conditions.
4. THE Platform SHALL support profile photo upload for Patient identity verification.
5. WHEN a Patient adds or updates health profile data, THE Platform SHALL timestamp and persist the change to the Health_Record.
6. IF a Patient attempts to register with an already-registered phone number or email, THEN THE Platform SHALL return a descriptive conflict error and prompt login.

---

### Requirement 2: Doctor Registration and Profile Management

**User Story:** As a doctor, I want to register and manage my professional profile, so that patients can discover and book consultations with me.

#### Acceptance Criteria

1. THE Platform SHALL allow a Doctor to register by providing full name, medical license number, specialty, years of experience, clinic address, and contact details.
2. WHEN a Doctor submits registration, THE Platform SHALL place the account in a pending state until admin verification of the medical license is complete.
3. WHEN a Doctor's license is verified, THE Platform SHALL activate the Doctor's profile and notify the Doctor via the Notification_Service.
4. THE Doctor SHALL be able to set consultation fees separately for virtual and physical Appointments.
5. THE Doctor SHALL be able to configure Availability_Slots for each day of the week.
6. THE Doctor SHALL be able to upload a profile photo, credentials, and a short professional bio.
7. IF a Doctor's medical license is found to be invalid during verification, THEN THE Platform SHALL reject the registration and notify the Doctor with a reason.

---

### Requirement 3: Doctor Discovery and Search

**User Story:** As a patient, I want to search and filter doctors by specialty, rating, location, and price, so that I can choose the most suitable doctor for my needs.

#### Acceptance Criteria

1. THE Platform SHALL provide a search interface allowing Patients to filter Doctors by medical specialty, Rating, consultation fee range, and availability.
2. WHEN a Patient enables location access, THE Platform SHALL sort Doctors by proximity using GPS_Location.
3. THE Platform SHALL display each Doctor's name, specialty, Rating, consultation fee, and distance from the Patient's GPS_Location.
4. WHEN a Patient selects a Doctor, THE Platform SHALL display the Doctor's full profile including bio, credentials, available Availability_Slots, and Patient reviews.
5. THE Platform SHALL update Doctor search results in real time when Availability_Slots change.
6. IF no Doctors match the applied filters, THEN THE Platform SHALL display a descriptive empty-state message and suggest broadening the search criteria.

---

### Requirement 4: Appointment Booking

**User Story:** As a patient, I want to book virtual and physical appointments with doctors, so that I can receive timely medical care.

#### Acceptance Criteria

1. WHEN a Patient selects an Availability_Slot, THE Platform SHALL create a pending Appointment and hold the slot for 10 minutes pending payment confirmation.
2. THE Platform SHALL support booking of both virtual Appointments (Telemedicine_Session) and physical Appointments at the Doctor's clinic.
3. WHEN a physical Appointment is booked, THE Platform SHALL display the Doctor's clinic address and provide GPS navigation directions.
4. WHEN an Appointment is confirmed, THE Platform SHALL send a confirmation notification to both the Patient and the Doctor via the Notification_Service.
5. WHEN an Appointment is within 30 minutes of its scheduled time, THE Platform SHALL send a reminder notification to the Patient and the Doctor.
6. IF a Patient cancels an Appointment more than 2 hours before the scheduled time, THEN THE Platform SHALL release the Availability_Slot and process any applicable refund.
7. IF a Patient cancels an Appointment less than 2 hours before the scheduled time, THEN THE Platform SHALL apply the cancellation policy defined by the Doctor.
8. THE Platform SHALL allow a Doctor to reschedule an Appointment and notify the Patient of the new time via the Notification_Service.

---

### Requirement 5: Telemedicine Consultations

**User Story:** As a patient, I want to consult with my doctor via video, audio, or chat, so that I can receive medical care without traveling.

#### Acceptance Criteria

1. WHEN a virtual Appointment time is reached, THE Platform SHALL initiate a Telemedicine_Session and notify both the Patient and Doctor to join.
2. THE Telemedicine_Session SHALL support video call, audio-only call, and text chat modes, selectable by the Patient or Doctor.
3. WHEN a Telemedicine_Session is active, THE Platform SHALL allow the Doctor to share files, images, and documents with the Patient within the session.
4. THE Platform SHALL encrypt all Telemedicine_Session data in transit using TLS 1.2 or higher.
5. WHEN a Telemedicine_Session ends, THE Platform SHALL generate a session summary and attach it to the Patient's Health_Record.
6. IF a network interruption occurs during a Telemedicine_Session, THEN THE Platform SHALL attempt to reconnect automatically for up to 60 seconds before displaying a reconnection prompt.
7. WHILE a Telemedicine_Session is active, THE Platform SHALL display the session duration to both the Patient and the Doctor.
8. THE Platform SHALL allow the Doctor to record a Telemedicine_Session only with explicit written consent from the Patient obtained before the session begins.

---

### Requirement 6: E-Prescription Management

**User Story:** As a doctor, I want to issue digital prescriptions to patients, so that they can order medication safely and legally through the platform.

#### Acceptance Criteria

1. WHEN a Doctor issues a Prescription, THE Platform SHALL record the medication name, dosage, frequency, duration, and any special instructions.
2. THE Platform SHALL attach each Prescription to the issuing Doctor's profile and the Patient's Health_Record.
3. WHEN a Prescription is issued, THE Platform SHALL notify the Patient via the Notification_Service.
4. THE Patient SHALL only be able to order medication from a Pharmacy if a valid, unexpired Prescription exists for that medication.
5. WHEN a Prescription is used to place a medication order, THE Platform SHALL mark the Prescription as dispensed and prevent duplicate orders for the same Prescription.
6. THE Platform SHALL allow a Doctor to set an expiry date on a Prescription, defaulting to 30 days from the issue date if not specified.
7. IF a Patient attempts to order medication without a valid Prescription, THEN THE Platform SHALL reject the order and display a descriptive error message.
8. THE Platform SHALL allow a Doctor to cancel a Prescription before it is dispensed, with a mandatory reason recorded.

---

### Requirement 7: Pharmacy Integration and Medication Ordering

**User Story:** As a patient, I want to order prescribed medication from a pharmacy and have it delivered, so that I can receive treatment without visiting a pharmacy in person.

#### Acceptance Criteria

1. WHEN a Patient initiates a medication order, THE Platform SHALL display only Pharmacies that have the prescribed medication in stock.
2. THE Platform SHALL sync Prescription data to the selected Pharmacy in real time upon order placement.
3. WHEN a Pharmacy receives an order, THE Platform SHALL notify the Pharmacy via the Notification_Service with the Prescription details and Patient delivery address.
4. THE Pharmacy SHALL be able to confirm, reject, or request clarification on a received order within the platform.
5. WHEN a Pharmacy confirms an order, THE Platform SHALL assign a Delivery_Agent and provide the Patient with a real-time delivery tracking link.
6. THE Platform SHALL support medication pickup at the Pharmacy as an alternative to delivery.
7. WHEN a medication order is delivered or picked up, THE Platform SHALL update the order status and notify the Patient.
8. THE Pharmacy SHALL be able to update medication stock levels in real time through the platform's inventory management interface.
9. IF a Pharmacy rejects an order, THEN THE Platform SHALL notify the Patient and suggest alternative Pharmacies with the medication in stock.

---

### Requirement 8: Payment, Insurance, and Credit

**User Story:** As a patient, I want to pay for consultations, medication, and lab tests using local payment methods, insurance, instalment plans, or a healthcare credit line, so that healthcare is financially accessible to me.

#### Acceptance Criteria

1. THE Payment_Gateway SHALL support local payment methods including mobile money, bank transfer, and debit/credit cards.
2. WHEN a Patient has active medical insurance, THE Platform SHALL allow the Patient to submit a claim to the Insurer for consultation and medication costs.
3. WHEN an insurance claim is submitted, THE Platform SHALL transmit the required claim data to the Insurer and display the claim status to the Patient.
4. THE Platform SHALL allow a Patient to pay for consultations, medication, and Lab_Tests on credit via a Credit_Line, subject to a credit limit determined by the Patient's Credit_Score.
5. THE Platform SHALL compute a Patient's Credit_Score based on the Patient's payment history on the platform, including on-time payments, missed payments, and outstanding balances.
6. WHEN a Patient's Credit_Score changes, THE Platform SHALL update the Patient's Credit_Line limit and notify the Patient via the Notification_Service.
7. WHEN a credit payment is made, THE Platform SHALL record the outstanding balance and send repayment reminders to the Patient via the Notification_Service.
8. WHEN a Patient's outstanding credit balance exceeds 80% of their Credit_Line limit, THE Platform SHALL send a balance warning to the Patient via the Notification_Service.
9. THE Platform SHALL offer Instalment_Plans for healthcare expenses exceeding a threshold defined by the platform operator, allowing the Patient to repay in equal periodic instalments.
10. WHEN a Patient selects an Instalment_Plan, THE Platform SHALL display the instalment amount, frequency, total repayable amount, and due dates before the Patient confirms.
11. WHEN an instalment payment is due, THE Platform SHALL send a reminder to the Patient via the Notification_Service at least 24 hours before the due date.
12. IF an instalment payment is missed, THEN THE Platform SHALL record the missed payment, apply any applicable late fee as configured by the platform operator, and notify the Patient.
13. WHEN a payment is completed, THE Platform SHALL generate a digital receipt and attach it to the Patient's transaction history.
14. IF a payment fails, THEN THE Platform SHALL notify the Patient with a descriptive error and retain the Appointment or order in a pending state for 10 minutes.
15. THE Platform SHALL display a full transaction history to the Patient, including consultation fees, medication costs, Lab_Test charges, insurance claims, credit balances, and Instalment_Plan schedules.

---

### Requirement 9: Medication Adherence Tracking

**User Story:** As a patient, I want to be reminded to take my medication and log my adherence, so that I can stay on track with my treatment plan.

#### Acceptance Criteria

1. WHEN a Prescription is dispensed, THE Platform SHALL automatically generate a Medication_Schedule based on the prescribed dosage and frequency.
2. WHEN a scheduled dose time is reached, THE Platform SHALL send a reminder notification to the Patient via the Notification_Service.
3. WHEN a Patient confirms a dose, THE Platform SHALL record an Adherence_Event with a timestamp and mark the dose as taken.
4. WHEN a Patient misses a dose by more than 2 hours past the scheduled time, THE Platform SHALL record an Adherence_Event as missed.
5. WHEN 3 consecutive doses are missed, THE Platform SHALL send an alert to the Patient's designated Next_of_Kin via the Notification_Service.
6. THE Platform SHALL display a weekly and monthly adherence summary to the Patient and the prescribing Doctor.
7. WHEN a Medication_Schedule is completed, THE Platform SHALL notify the Patient and the prescribing Doctor.
8. IF a Patient's Medication_Schedule overlaps with a known drug interaction, THEN THE Platform SHALL alert the prescribing Doctor before the Prescription is finalized.

---

### Requirement 10: Emergency Contact (Next of Kin) Management

**User Story:** As a patient, I want to designate emergency contacts, so that my loved ones can be notified in a medical emergency.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to add up to 3 Next_of_Kin contacts, each with a name, relationship, phone number, and email address.
2. WHEN a Next_of_Kin contact is added, THE Platform SHALL send a notification to the contact informing them of their designation.
3. THE Doctor SHALL be able to send an emergency alert to a Patient's Next_of_Kin directly from the platform during a consultation.
4. WHEN an emergency alert is triggered, THE Platform SHALL contact all of the Patient's designated Next_of_Kin simultaneously via SMS and push notification.
5. WHEN 3 consecutive medication doses are missed, THE Platform SHALL automatically alert the Patient's Next_of_Kin as defined in Requirement 9.
6. THE Platform SHALL log all emergency alerts with a timestamp, trigger reason, and delivery status for audit purposes.
7. IF a Next_of_Kin notification fails to deliver, THEN THE Platform SHALL retry delivery up to 3 times at 5-minute intervals and log the final delivery status.

---

### Requirement 11: Patient Health Records

**User Story:** As a patient, I want a centralized health record, so that my doctors have full context of my medical history.

#### Acceptance Criteria

1. THE Platform SHALL maintain a longitudinal Health_Record for each Patient, including consultation history, Prescriptions, diagnoses, allergies, and vitals.
2. WHEN a Doctor completes a consultation, THE Platform SHALL prompt the Doctor to add clinical notes, diagnosis codes, and any issued Prescriptions to the Patient's Health_Record.
3. THE Patient SHALL be able to view their own Health_Record at any time.
4. THE Patient SHALL be able to grant or revoke a specific Doctor's access to their Health_Record.
5. WHEN a Doctor is granted access to a Patient's Health_Record, THE Platform SHALL log the access grant with a timestamp.
6. THE Platform SHALL allow a Patient to download their Health_Record as a PDF document.
7. IF a Doctor attempts to access a Patient's Health_Record without granted access, THEN THE Platform SHALL deny the request and log the attempt.

---

### Requirement 12: Doctor Dashboard and Analytics

**User Story:** As a doctor, I want a dashboard with analytics on my patients and revenue, so that I can manage my practice effectively.

#### Acceptance Criteria

1. THE Platform SHALL provide a Doctor with a dashboard displaying upcoming Appointments, recent consultations, and pending Prescriptions.
2. THE Platform SHALL display revenue analytics to the Doctor, including total earnings, earnings by period, and earnings by consultation type.
3. THE Platform SHALL display patient analytics to the Doctor, including total active patients, new patients per period, and consultation frequency.
4. WHEN a Doctor views a patient list, THE Platform SHALL display each Patient's name, last consultation date, and active Prescriptions.
5. THE Platform SHALL allow a Doctor to export analytics data as a CSV file.
6. THE Platform SHALL update dashboard data within 5 minutes of any new consultation, payment, or appointment event.

---

### Requirement 13: Pharmacy Dashboard and Workflow

**User Story:** As a pharmacy, I want a real-time dashboard to manage incoming prescriptions and delivery workflows, so that I can fulfill orders efficiently.

#### Acceptance Criteria

1. THE Platform SHALL provide a Pharmacy with a real-time dashboard displaying incoming orders, order statuses, and inventory levels.
2. WHEN a new order arrives, THE Platform SHALL display the Prescription details, Patient delivery address, and payment status on the Pharmacy dashboard.
3. THE Pharmacy SHALL be able to update the status of an order through the stages: received, preparing, ready for pickup/dispatch, and delivered.
4. WHEN an order status changes, THE Platform SHALL notify the Patient of the updated status via the Notification_Service.
5. THE Platform SHALL allow the Pharmacy to manage medication inventory, including adding new stock, updating quantities, and marking items as out of stock.
6. WHEN a medication's stock level falls below a threshold set by the Pharmacy, THE Platform SHALL send a low-stock alert to the Pharmacy.
7. THE Platform SHALL provide the Pharmacy with a daily summary report of fulfilled orders, revenue, and pending deliveries.

---

### Requirement 14: AI-Powered Health Assistant

**User Story:** As a patient, I want an AI assistant to help me understand my symptoms and navigate the platform, so that I can make informed decisions about my care.

#### Acceptance Criteria

1. THE AI_Assistant SHALL provide a symptom checker that collects Patient-reported symptoms and suggests possible conditions and recommended specialist types.
2. WHEN a Patient completes a symptom check, THE AI_Assistant SHALL recommend relevant Doctors from the platform based on the suggested specialist type.
3. THE AI_Assistant SHALL answer general health and medication questions using verified medical knowledge sources.
4. WHEN a Patient asks a question that requires clinical judgment, THE AI_Assistant SHALL clearly state that the response is informational only and recommend consulting a Doctor.
5. THE AI_Assistant SHALL support text and voice input from the Patient.
6. THE Platform SHALL log all AI_Assistant interactions and make them available to the Patient for review.
7. IF the AI_Assistant detects keywords indicating a medical emergency in a Patient's input, THEN THE Platform SHALL immediately display emergency contact options and prompt the Patient to call emergency services.

---

### Requirement 15: Ratings and Reviews

**User Story:** As a patient, I want to rate and review doctors after consultations, so that other patients can make informed choices.

#### Acceptance Criteria

1. WHEN a consultation is marked as complete, THE Platform SHALL prompt the Patient to submit a Rating (1–5) and an optional written review for the Doctor.
2. THE Platform SHALL display the average Rating and total review count on each Doctor's profile.
3. WHEN a new Rating is submitted, THE Platform SHALL update the Doctor's average Rating within 5 minutes.
4. THE Platform SHALL allow a Doctor to respond to a Patient review publicly.
5. IF a review contains prohibited content as defined by the platform's content policy, THEN THE Platform SHALL flag the review for moderation before publishing.
6. THE Patient SHALL be able to submit only one Rating per completed Appointment.

---

### Requirement 16: Notifications and Communication

**User Story:** As a user, I want timely and relevant notifications, so that I stay informed about my health activities on the platform.

#### Acceptance Criteria

1. THE Notification_Service SHALL support push notifications, SMS, and email as delivery channels.
2. THE Patient SHALL be able to configure notification preferences, including enabling or disabling each channel for each notification type.
3. WHEN a notification is sent, THE Notification_Service SHALL log the delivery status, timestamp, and channel used.
4. THE Platform SHALL send notifications for the following events: Appointment confirmations, Appointment reminders, Prescription issuance, medication dose reminders, order status updates, and emergency alerts.
5. IF a push notification fails to deliver, THEN THE Notification_Service SHALL fall back to SMS delivery for critical notifications (emergency alerts and medication reminders).

---

### Requirement 17: Security, Privacy, and Compliance

**User Story:** As a user, I want my health data to be secure and private, so that I can trust the platform with sensitive medical information.

#### Acceptance Criteria

1. THE Platform SHALL encrypt all Patient health data at rest using AES-256 encryption.
2. THE Platform SHALL encrypt all data in transit using TLS 1.2 or higher.
3. THE Platform SHALL enforce multi-factor authentication for Doctor and Pharmacy accounts.
4. WHEN a Patient account is accessed from a new device, THE Platform SHALL require identity verification before granting access.
5. THE Platform SHALL maintain an audit log of all access to Patient Health_Records, including the accessing user, timestamp, and action performed.
6. THE Platform SHALL comply with applicable health data regulations (e.g., HIPAA, GDPR, or local equivalents) as configured by the platform operator.
7. THE Patient SHALL be able to request deletion of their account and personal data, subject to legal retention requirements.
8. IF 5 consecutive failed login attempts are detected for an account, THEN THE Platform SHALL temporarily lock the account and notify the account owner via the Notification_Service.

---

### Requirement 18: IoT Device Integration (Future Phase)

**User Story:** As a patient with a chronic condition, I want my health monitoring device to sync with the platform, so that my doctor can track my vitals remotely.

#### Acceptance Criteria

1. WHERE an IoT_Device is connected, THE Platform SHALL receive and store biometric readings including blood pressure, blood glucose, heart rate, and oxygen saturation.
2. WHEN an IoT_Device transmits a reading, THE Platform SHALL attach the reading to the Patient's Health_Record with a timestamp and device identifier.
3. WHEN a biometric reading falls outside the normal range defined for the Patient, THE Platform SHALL alert the Patient's assigned Doctor via the Notification_Service.
4. WHEN a critical biometric reading is detected, THE Platform SHALL simultaneously alert the Patient, the Doctor, and the Patient's Next_of_Kin.
5. THE Platform SHALL display a time-series chart of IoT_Device readings on the Patient's Health_Record and the Doctor's dashboard.
6. THE Platform SHALL support integration with standard health device protocols (e.g., Bluetooth LE, HL7 FHIR).

---

### Requirement 19: Emergency Response Integration (Future Phase)

**User Story:** As a patient in a medical emergency, I want the platform to automatically alert emergency services and my next of kin, so that I receive immediate help.

#### Acceptance Criteria

1. WHERE an IoT_Device is connected and a Patient collapse event is detected, THE Platform SHALL simultaneously alert the Patient's Next_of_Kin and local emergency services.
2. WHEN an emergency alert is triggered, THE Platform SHALL transmit the Patient's GPS_Location, Health_Record summary, and active Prescriptions to the responding emergency service.
3. THE Platform SHALL provide a manual SOS button that a Patient can activate to trigger an emergency alert without an IoT_Device.
4. WHEN a manual SOS is activated, THE Platform SHALL alert the Patient's Next_of_Kin and display the Patient's GPS_Location to them in real time.
5. WHEN an emergency alert is resolved, THE Platform SHALL log the resolution time, responding party, and outcome for audit purposes.

---

### Requirement 20: Multi-Language and Accessibility Support

**User Story:** As a user in a diverse region, I want the platform to support multiple languages and accessibility features, so that healthcare is accessible to everyone.

#### Acceptance Criteria

1. THE Platform SHALL support a minimum of 2 languages at launch, with the ability to add additional languages without code changes.
2. THE Patient SHALL be able to select their preferred language from the platform settings, and THE Platform SHALL apply the selection to all UI text immediately.
3. THE Platform SHALL support screen reader compatibility for visually impaired users on both mobile and web interfaces.
4. THE Platform SHALL provide text size adjustment options in the accessibility settings.
5. THE AI_Assistant SHALL respond in the Patient's selected platform language.

### Requirement 21: Lab Test Ordering

**User Story:** As a patient, I want to order lab tests through the platform and receive results digitally, so that diagnostics are integrated into my health record.

#### Acceptance Criteria

1. WHEN a Doctor orders a Lab_Test during or after a consultation, THE Platform SHALL transmit the order to a selected Lab_Partner and attach the order to the Patient's Health_Record.
2. THE Patient SHALL be able to request a Lab_Test directly through the platform, subject to Doctor approval before the order is dispatched to a Lab_Partner.
3. THE Platform SHALL display available Lab_Partners to the Patient filtered by test type, proximity using GPS_Location, and price.
4. WHEN a Lab_Result is ready, THE Lab_Partner SHALL upload the result to the platform and THE Platform SHALL attach the Lab_Result to the Patient's Health_Record.
5. WHEN a Lab_Result is attached to a Health_Record, THE Platform SHALL notify the Patient and the ordering Doctor via the Notification_Service.
6. THE Patient SHALL be able to view and download their Lab_Results from the Health_Record at any time.
7. WHEN a Radiology_Report is produced, THE Lab_Partner SHALL upload the report and associated imaging files to the platform and THE Platform SHALL attach them to the Patient's Health_Record.
8. THE Doctor SHALL be able to view and annotate Lab_Results and Radiology_Reports within the platform and share annotations with the Patient.
9. IF a Lab_Result contains a critical value as flagged by the Lab_Partner, THEN THE Platform SHALL immediately alert the ordering Doctor via the Notification_Service.

---

### Requirement 22: Mental Health Services

**User Story:** As a patient, I want access to mental health support including therapy, mood tracking, and crisis escalation, so that my emotional wellbeing is cared for on the platform.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to book a Therapy_Session with a licensed therapist using the same Appointment booking flow defined in Requirement 4.
2. WHEN a Therapy_Session is completed, THE Platform SHALL attach a session summary to the Patient's Health_Record, visible only to the Patient and the therapist unless the Patient grants broader access.
3. THE Platform SHALL provide a mood tracking feature that allows a Patient to record a Mood_Log entry at any time, including an emotional state rating (1–5) and optional free-text notes.
4. THE Platform SHALL display a time-series chart of a Patient's Mood_Log entries to the Patient and, with the Patient's consent, to the Patient's therapist.
5. WHEN a Patient's Mood_Log entries show a rating of 1 for 3 consecutive days, THE Platform SHALL prompt the Patient with mental health resources and the option to book a Therapy_Session.
6. IF a Patient's input to the AI_Assistant or Mood_Log contains keywords indicating a mental health crisis, THEN THE Platform SHALL trigger the Crisis_Protocol.
7. WHEN the Crisis_Protocol is triggered, THE Platform SHALL immediately display crisis helpline numbers, prompt the Patient to contact emergency services, and notify the Patient's designated Next_of_Kin via the Notification_Service.
8. THE Platform SHALL allow a Patient to designate a mental health emergency contact separately from the general Next_of_Kin defined in Requirement 10.

---

### Requirement 23: Dental and Optical Consultations

**User Story:** As a patient, I want to book dental and optical specialist consultations and order corrective eyewear through the platform, so that my full healthcare needs are met in one place.

#### Acceptance Criteria

1. THE Platform SHALL support Dental_Consultation and Optical_Consultation as distinct appointment types, bookable through the same Appointment booking flow defined in Requirement 4.
2. WHEN a Dental_Consultation or Optical_Consultation is completed, THE Platform SHALL prompt the Doctor to attach clinical notes and any issued Prescriptions to the Patient's Health_Record.
3. WHEN an Optical_Consultation results in an Optical_Prescription, THE Platform SHALL attach the Optical_Prescription to the Patient's Health_Record and notify the Patient via the Notification_Service.
4. THE Platform SHALL allow a Patient to order prescription glasses or contact lenses from integrated optical partners using a valid Optical_Prescription.
5. WHEN a glasses or contact lens order is placed, THE Platform SHALL display the order status and estimated delivery date to the Patient.
6. IF an Optical_Prescription has expired, THEN THE Platform SHALL prevent the Patient from placing an eyewear order and prompt the Patient to book a new Optical_Consultation.

---

### Requirement 24: Maternal Health

**User Story:** As a pregnant patient, I want to track my pregnancy, receive fetal monitoring reminders, and manage my birth plan through the platform, so that my antenatal care is organized and accessible.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to create an Antenatal_Record, capturing the estimated due date, gestational age, and assigned obstetric Doctor.
2. WHEN an Antenatal_Record is created, THE Platform SHALL generate a schedule of recommended antenatal checkup Appointments based on standard obstetric guidelines and notify the Patient via the Notification_Service.
3. WHEN an antenatal checkup is completed, THE Doctor SHALL be able to record fetal measurements, maternal vitals, and clinical notes directly into the Antenatal_Record.
4. THE Platform SHALL send fetal monitoring reminders to the Patient at clinically recommended intervals as configured by the assigned obstetric Doctor.
5. THE Platform SHALL allow a Patient to create and update a Birth_Plan, capturing preferences for labour, delivery method, pain management, and postnatal care.
6. WHEN a Birth_Plan is updated, THE Platform SHALL notify the assigned obstetric Doctor via the Notification_Service.
7. THE Patient SHALL be able to share the Antenatal_Record and Birth_Plan with any Doctor on the platform by granting explicit access.
8. WHEN the estimated due date is within 4 weeks, THE Platform SHALL increase the frequency of reminder notifications to the Patient and the assigned obstetric Doctor.

---

### Requirement 25: Pediatric Care

**User Story:** As a parent or guardian, I want to manage my child's health records, vaccination schedule, and growth tracking through the platform, so that my child's healthcare is organized and monitored.

#### Acceptance Criteria

1. THE Platform SHALL allow a Guardian to create one or more Child_Profiles linked to the Guardian's Patient account, capturing the child's name, date of birth, blood type, and known allergies.
2. THE Platform SHALL maintain a separate Health_Record for each Child_Profile, accessible to the Guardian and any Doctor granted access by the Guardian.
3. WHEN a Doctor completes a consultation for a Child_Profile, THE Platform SHALL prompt the Doctor to record clinical notes, diagnoses, and Prescriptions in the child's Health_Record.
4. THE Platform SHALL generate a Vaccination_Record for each Child_Profile and display the recommended vaccination schedule based on the child's date of birth and national immunization guidelines.
5. WHEN a vaccination is due within 7 days, THE Platform SHALL send a reminder to the Guardian via the Notification_Service.
6. WHEN a vaccination is administered, THE Doctor or Guardian SHALL be able to record the vaccine name, date, batch number, and administering provider in the Vaccination_Record.
7. THE Platform SHALL allow a Guardian to record Growth_Entry measurements for a child, including height, weight, and developmental milestones.
8. THE Platform SHALL display a growth chart visualizing the child's Growth_Entry history against age-appropriate reference percentiles.
9. IF a child's recorded growth measurements fall outside the expected range for their age, THEN THE Platform SHALL alert the Guardian and recommend a consultation with a pediatric Doctor.

---

### Requirement 26: Surgical Referral Management

**User Story:** As a doctor, I want to refer patients to specialists or hospitals within the platform and track the referral status, so that care transitions are coordinated and visible to the patient.

#### Acceptance Criteria

1. WHEN a Doctor creates a Referral, THE Platform SHALL record the referring Doctor, the receiving Doctor or hospital, the reason for referral, and the relevant sections of the Patient's Health_Record included in the Referral.
2. WHEN a Referral is created, THE Platform SHALL notify the receiving Doctor or hospital and the Patient via the Notification_Service.
3. THE Patient SHALL be able to view all active and historical Referrals and their Referral_Status from the Health_Record.
4. THE receiving Doctor SHALL be able to accept or decline a Referral within the platform, with a mandatory reason recorded for any decline.
5. WHEN a Referral_Status changes, THE Platform SHALL notify the referring Doctor and the Patient via the Notification_Service.
6. WHEN a referred consultation is completed, THE receiving Doctor SHALL be able to send a consultation summary back to the referring Doctor through the platform.
7. IF a Referral has not received a response from the receiving Doctor within 48 hours, THEN THE Platform SHALL send a follow-up reminder to the receiving Doctor via the Notification_Service.

---

### Requirement 27: Personal Health and Wellness Tracking

**User Story:** As a patient, I want to set personal health goals, track wellness metrics, and follow structured care plans for chronic conditions, so that I can proactively manage my health.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to create Health_Goals for measurable metrics including daily steps, target weight, sleep duration, and daily water intake.
2. WHEN a Patient records a wellness metric entry, THE Platform SHALL compare the entry against the Patient's active Health_Goals and display progress toward each goal.
3. THE Platform SHALL display a time-series chart of each wellness metric tracked by the Patient.
4. THE Platform SHALL maintain a Vaccination_Record for adult Patients and display recommended vaccinations based on age, health profile, and national guidelines.
5. WHEN a recommended vaccination is due, THE Platform SHALL send a reminder to the Patient via the Notification_Service.
6. WHEN a vaccination is administered, THE Doctor or Patient SHALL be able to record the vaccine name, date, batch number, and administering provider in the Vaccination_Record.
7. THE Platform SHALL allow a Doctor to assign a Care_Plan to a Patient for managing a chronic condition, including structured tasks, monitoring targets, and review intervals.
8. WHEN a Care_Plan task is due, THE Platform SHALL send a reminder to the Patient via the Notification_Service.
9. WHEN a Patient completes a Care_Plan task, THE Platform SHALL record the completion with a timestamp and update the Care_Plan progress visible to both the Patient and the assigned Doctor.
10. WHEN a Care_Plan review interval is reached, THE Platform SHALL notify the assigned Doctor to review the Patient's progress and update the Care_Plan as needed.

---

### Requirement 28: Second Opinion Requests

**User Story:** As a patient, I want to share my health record with another doctor for a second opinion, so that I can make informed decisions about my treatment without changing my primary doctor.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient to submit a Second_Opinion request by selecting a Doctor and specifying which sections of the Health_Record to share.
2. WHEN a Second_Opinion request is submitted, THE Platform SHALL notify the selected Doctor via the Notification_Service and grant the Doctor read-only access to the specified Health_Record sections for the duration of the Second_Opinion.
3. THE Doctor receiving a Second_Opinion request SHALL be able to accept or decline the request within the platform, with a mandatory reason recorded for any decline.
4. WHEN a Second_Opinion Doctor accepts the request, THE Platform SHALL allow the Doctor to submit written clinical notes and recommendations to the Patient.
5. WHEN a Second_Opinion response is submitted, THE Platform SHALL notify the Patient via the Notification_Service and attach the response to the Patient's Health_Record.
6. THE Second_Opinion Doctor SHALL NOT be able to issue Prescriptions, modify the Health_Record, or assume the role of primary treating Doctor through the Second_Opinion workflow.
7. WHEN a Second_Opinion is completed or declined, THE Platform SHALL revoke the Second_Opinion Doctor's access to the shared Health_Record sections and log the access revocation with a timestamp.

---

### Requirement 29: Admin Panel

**User Story:** As a platform operator, I want an admin dashboard for user management, license verification, dispute resolution, and analytics, so that I can operate and govern the platform effectively.

#### Acceptance Criteria

1. THE Platform SHALL provide an Admin with a dashboard displaying platform-wide metrics including total registered Patients, Doctors, Pharmacies, and Lab_Partners, as well as daily active users and transaction volumes.
2. THE Admin SHALL be able to view, approve, suspend, or permanently deactivate any Doctor, Pharmacy, or Lab_Partner account from the admin dashboard.
3. WHEN a Doctor registration is submitted, THE Platform SHALL queue the license verification task in the Admin dashboard for review.
4. THE Admin SHALL be able to record the outcome of a license verification, including approval, rejection, and the reason for rejection, and THE Platform SHALL notify the affected Doctor accordingly.
5. THE Admin SHALL be able to view and manage reported disputes between Patients, Doctors, and Pharmacies, including the ability to add resolution notes and close disputes.
6. WHEN a dispute is closed by an Admin, THE Platform SHALL notify all parties involved via the Notification_Service.
7. THE Admin SHALL be able to configure platform-wide settings including credit thresholds, instalment plan parameters, cancellation policies, and content moderation rules.
8. THE Platform SHALL provide the Admin with exportable analytics reports covering consultation volumes, revenue, user growth, and geographic distribution.
9. IF an Admin account is accessed from an unrecognized device, THEN THE Platform SHALL require multi-factor authentication before granting access.

---

### Requirement 30: Doctor-to-Doctor Referral System

**User Story:** As a doctor, I want to refer patients to other doctors or specialists on the platform with relevant clinical context, so that care handoffs are seamless and well-informed.

#### Acceptance Criteria

1. WHEN a Doctor creates a doctor-to-doctor Referral, THE Platform SHALL allow the referring Doctor to select specific sections of the Patient's Health_Record and attach clinical notes to include in the Referral.
2. THE Patient SHALL provide explicit consent before any Health_Record sections are shared as part of a Referral, and THE Platform SHALL record the consent with a timestamp.
3. WHEN a Referral is sent, THE Platform SHALL notify the receiving Doctor via the Notification_Service with a summary of the referral reason and the Patient's relevant clinical context.
4. THE receiving Doctor SHALL be able to accept, decline, or request additional information for a Referral within the platform.
5. WHEN a receiving Doctor requests additional information, THE Platform SHALL notify the referring Doctor and allow the referring Doctor to respond within the platform.
6. WHEN a Referral is accepted, THE Platform SHALL grant the receiving Doctor access to the shared Health_Record sections and log the access grant with a timestamp.
7. WHEN the referred consultation is completed, THE receiving Doctor SHALL submit a consultation summary to the referring Doctor through the platform, and THE Platform SHALL attach the summary to the Patient's Health_Record.
8. WHEN a Referral is completed or declined, THE Platform SHALL revoke the receiving Doctor's access to the shared Health_Record sections and update the Referral_Status accordingly.

---

### Requirement 31: Physical Clinic Queue Management

**User Story:** As a patient attending a physical clinic, I want to join a virtual queue and check my position and estimated wait time remotely, so that I can manage my time without waiting in the clinic.

#### Acceptance Criteria

1. THE Platform SHALL allow a Patient with a confirmed physical Appointment to join a virtual queue for the clinic, creating a Queue_Entry with the Patient's name, Appointment time, and arrival status.
2. WHEN a Patient joins the virtual queue, THE Platform SHALL assign a queue position and compute an Estimated_Wait_Time based on the number of patients ahead and the Doctor's average consultation duration.
3. THE Platform SHALL display the Patient's current queue position and Estimated_Wait_Time in real time, updating at least every 2 minutes.
4. WHEN a Patient's queue position reaches 2nd place, THE Platform SHALL send a notification to the Patient via the Notification_Service prompting them to proceed to the clinic.
5. THE Doctor or clinic staff SHALL be able to advance the queue, mark a Patient as seen, or mark a Patient as absent through the platform.
6. WHEN a Patient is marked as absent, THE Platform SHALL remove the Patient's Queue_Entry and notify the Patient via the Notification_Service.
7. WHEN a Patient is marked as seen, THE Platform SHALL record the actual wait time against the Estimated_Wait_Time for that Queue_Entry for analytics purposes.
8. IF a Doctor's schedule is delayed by more than 15 minutes, THEN THE Platform SHALL recalculate all Estimated_Wait_Times in the queue and notify affected Patients of the updated estimate via the Notification_Service.
