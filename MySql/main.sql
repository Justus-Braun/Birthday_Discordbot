#Create Database
Create database Birthday_DiscordBot;
use Birthday_DiscordBot;

#Create Table user
create table user(
	ID int primary key auto_increment,
    username varchar(30),
    birthday date
);

#Query through user table
select * from user;