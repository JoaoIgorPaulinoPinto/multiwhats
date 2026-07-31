namespace multiwhats_api.src.data;

public static class EntityConstraints
{
    public const int UserNameMaxLength = 200;
    public const int UserPasswordMinLength = 6;

    public const int ChatJidMaxLength = 100;
    public const int ChatPhoneNumberMaxLength = 20;
    public const int ChatNameMaxLength = 150;

    public const int ContactJidMaxLength = 100;
    public const int ContactPhoneNumberMaxLength = 20;
    public const int ContactNameMaxLength = 150;
    public const int ContactPushNameMaxLength = 150;
    public const int ContactProfilePicUrlMaxLength = 500;

    public const int OccurrenceTitleMaxLength = 200;
    public const int OccurrenceDescriptionMaxLength = 2000;

    public const int MessageBodyMaxLength = 4000;
    public const int MessageFromJidMaxLength = 100;
    public const int MessageToJidMaxLength = 100;
    public const int MessagePhoneNumberMaxLength = 20;
    public const int MessageNotifyNameMaxLength = 100;
    public const int MessageMediaUrlMaxLength = 500;
    public const int MessageMediaMimeTypeMaxLength = 100;
    public const int MessageMediaFilenameMaxLength = 255;
    public const int MessageMediaCaptionMaxLength = 500;
    public const int MessageWhatsAppIdMaxLength = 200;

    public const int ClientNameMaxLength = 200;
    public const int ClientMainPhoneNumberMaxLength = 20;

    public const int TaskTitleMaxLength = 300;
    public const int TaskDescriptionMaxLength = 2000;

    public const int GroupNameMaxLength = 200;
    public const int GroupDescriptionMaxLength = 1000;
    public const int GroupWhatsAppGroupIdMaxLength = 100;

    public const int AuditEntityTypeMaxLength = 100;
    public const int AuditActionMaxLength = 50;
    public const int AuditDescriptionMaxLength = 1000;
    public const int AuditUserNameMaxLength = 200;
    public const int AuditUserRoleMaxLength = 50;

    public const int RegistrationCodeValueMaxLength = 50;

    public const int JidMaxLength = 100;
    public const int PhoneNumberMaxLength = 20;

    public const int EnumMaxLength = 20;
}
