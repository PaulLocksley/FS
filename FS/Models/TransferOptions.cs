namespace FS.Models;

public enum TransferOptions
{
    email_me_copies,
    email_me_on_expire,
    email_upload_complete,
    email_download_complete,
    email_daily_statistics,
    email_report_on_closing,
    enable_recipient_email_download_complete,
    add_me_to_recipients,
    email_recipient_when_transfer_expires,
    get_a_link,
    hide_sender_email,
    redirect_url_on_complete,
    encryption,
    collection,
    must_be_logged_in_to_download,
}