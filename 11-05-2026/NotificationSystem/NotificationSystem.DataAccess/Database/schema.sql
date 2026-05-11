create database notification_system;

use notification_system;

create table users(
id serial not null primary key,
name varchar(50) not null,
email varchar(50) unique not null,
phone_num varchar(20) not null,
isActive boolean default true,
ReceiveEmailNotification boolean default true,
ReceiveSmsNotification boolean default true,
CreatedAt timestamp default current_timestamp
);


create type notification_type as enum ('Email', 'Sms');

create table notifications(
id serial not null primary key,
user_id int not null,
user_name varchar(50) not null,
type notification_type not null,
Subject varchar(100),
Message varchar(200) not null,
Recipient varchar(100) not null,
Is_sent boolean default false,
Sent_at timestamp default current_timestamp,
Error_message text,
created_at timestamp default current_timestamp,

constraint fk_notitication_user
foreign key (user_id)
references users(id)
on delete cascade

);

create index idx_notification_user_id on notifications(user_id);
create index idx_notification_type on notifications(type);
create index idx_notification_is_sent on notifications(is_sent);
create index idx_notification_sent_at on notifications(sent_at);
