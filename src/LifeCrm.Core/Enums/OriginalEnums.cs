namespace LifeCrm.Core.Enums;

public enum UserRole          { Viewer, Manager, Finance, Admin }
public enum CampaignStatus    { Draft, Active, Paused, Completed, Cancelled }
public enum ContactType       { Individual, Organization, Church, Foundation }
public enum DecisionType      { Salvation, Recommitment, WaterBaptism, HolySpiritBaptism, Healing, Deliverance, Other }
public enum DocumentType      { DonationReceipt, DonationSummary }
public enum DonationStatus    { Pending, Confirmed, Refunded, Voided, Cancelled }
public enum InteractionType   { Call, Email, Meeting, Note, Other }
public enum NewsletterStatus  { Draft, Sent }
public enum PrayerStatus      { Active, Answered, Withdrawn }
public enum ProjectStatus     { Planning, Active, OnHold, Completed, Archived }
public enum ReportLanguage    { English, Swedish }
public enum ReportStatus      { Draft, Submitted, Approved, ReturnedForRevision }
