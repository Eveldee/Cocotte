﻿// <auto-generated />
using System;
using Cocotte.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Cocotte.Migrations
{
    [DbContext(typeof(CocotteDbContext))]
    [Migration("20230326133102_AddActivityIsClosed")]
    partial class AddActivityIsClosed
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzBlobTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerName")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<byte[]>("BlobData")
                        .HasColumnType("bytea")
                        .HasColumnName("BLOB_DATA");

                    b.HasKey("SchedulerName", "TriggerName", "TriggerGroup");

                    b.ToTable("QRTZ_BLOB_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzCalendar", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("CalendarName")
                        .HasColumnType("text")
                        .HasColumnName("CALENDAR_NAME");

                    b.Property<byte[]>("Calendar")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("CALENDAR");

                    b.HasKey("SchedulerName", "CalendarName");

                    b.ToTable("QRTZ_CALENDARS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzCronTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerName")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<string>("CronExpression")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("CRON_EXPRESSION");

                    b.Property<string>("TimeZoneId")
                        .HasColumnType("text")
                        .HasColumnName("TIME_ZONE_ID");

                    b.HasKey("SchedulerName", "TriggerName", "TriggerGroup");

                    b.ToTable("QRTZ_CRON_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzFiredTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("EntryId")
                        .HasColumnType("text")
                        .HasColumnName("ENTRY_ID");

                    b.Property<long>("FiredTime")
                        .HasColumnType("bigint")
                        .HasColumnName("FIRED_TIME");

                    b.Property<string>("InstanceName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("INSTANCE_NAME");

                    b.Property<bool>("IsNonConcurrent")
                        .HasColumnType("bool")
                        .HasColumnName("IS_NONCONCURRENT");

                    b.Property<string>("JobGroup")
                        .HasColumnType("text")
                        .HasColumnName("JOB_GROUP");

                    b.Property<string>("JobName")
                        .HasColumnType("text")
                        .HasColumnName("JOB_NAME");

                    b.Property<int>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("PRIORITY");

                    b.Property<bool?>("RequestsRecovery")
                        .HasColumnType("bool")
                        .HasColumnName("REQUESTS_RECOVERY");

                    b.Property<long>("ScheduledTime")
                        .HasColumnType("bigint")
                        .HasColumnName("SCHED_TIME");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("STATE");

                    b.Property<string>("TriggerGroup")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<string>("TriggerName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.HasKey("SchedulerName", "EntryId");

                    b.HasIndex("InstanceName")
                        .HasDatabaseName("IDX_QRTZ_FT_TRIG_INST_NAME");

                    b.HasIndex("JobGroup")
                        .HasDatabaseName("IDX_QRTZ_FT_JOB_GROUP");

                    b.HasIndex("JobName")
                        .HasDatabaseName("IDX_QRTZ_FT_JOB_NAME");

                    b.HasIndex("RequestsRecovery")
                        .HasDatabaseName("IDX_QRTZ_FT_JOB_REQ_RECOVERY");

                    b.HasIndex("TriggerGroup")
                        .HasDatabaseName("IDX_QRTZ_FT_TRIG_GROUP");

                    b.HasIndex("TriggerName")
                        .HasDatabaseName("IDX_QRTZ_FT_TRIG_NAME");

                    b.HasIndex("SchedulerName", "TriggerName", "TriggerGroup")
                        .HasDatabaseName("IDX_QRTZ_FT_TRIG_NM_GP");

                    b.ToTable("QRTZ_FIRED_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzJobDetail", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("JobName")
                        .HasColumnType("text")
                        .HasColumnName("JOB_NAME");

                    b.Property<string>("JobGroup")
                        .HasColumnType("text")
                        .HasColumnName("JOB_GROUP");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("DESCRIPTION");

                    b.Property<bool>("IsDurable")
                        .HasColumnType("bool")
                        .HasColumnName("IS_DURABLE");

                    b.Property<bool>("IsNonConcurrent")
                        .HasColumnType("bool")
                        .HasColumnName("IS_NONCONCURRENT");

                    b.Property<bool>("IsUpdateData")
                        .HasColumnType("bool")
                        .HasColumnName("IS_UPDATE_DATA");

                    b.Property<string>("JobClassName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("JOB_CLASS_NAME");

                    b.Property<byte[]>("JobData")
                        .HasColumnType("bytea")
                        .HasColumnName("JOB_DATA");

                    b.Property<bool>("RequestsRecovery")
                        .HasColumnType("bool")
                        .HasColumnName("REQUESTS_RECOVERY");

                    b.HasKey("SchedulerName", "JobName", "JobGroup");

                    b.HasIndex("RequestsRecovery")
                        .HasDatabaseName("IDX_QRTZ_J_REQ_RECOVERY");

                    b.ToTable("QRTZ_JOB_DETAILS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzLock", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("LockName")
                        .HasColumnType("text")
                        .HasColumnName("LOCK_NAME");

                    b.HasKey("SchedulerName", "LockName");

                    b.ToTable("QRTZ_LOCKS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzPausedTriggerGroup", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.HasKey("SchedulerName", "TriggerGroup");

                    b.ToTable("QRTZ_PAUSED_TRIGGER_GRPS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzSchedulerState", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("InstanceName")
                        .HasColumnType("text")
                        .HasColumnName("INSTANCE_NAME");

                    b.Property<long>("CheckInInterval")
                        .HasColumnType("bigint")
                        .HasColumnName("CHECKIN_INTERVAL");

                    b.Property<long>("LastCheckInTime")
                        .HasColumnType("bigint")
                        .HasColumnName("LAST_CHECKIN_TIME");

                    b.HasKey("SchedulerName", "InstanceName");

                    b.ToTable("QRTZ_SCHEDULER_STATE", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzSimplePropertyTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerName")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<bool?>("BooleanProperty1")
                        .HasColumnType("bool")
                        .HasColumnName("BOOL_PROP_1");

                    b.Property<bool?>("BooleanProperty2")
                        .HasColumnType("bool")
                        .HasColumnName("BOOL_PROP_2");

                    b.Property<decimal?>("DecimalProperty1")
                        .HasColumnType("numeric")
                        .HasColumnName("DEC_PROP_1");

                    b.Property<decimal?>("DecimalProperty2")
                        .HasColumnType("numeric")
                        .HasColumnName("DEC_PROP_2");

                    b.Property<int?>("IntegerProperty1")
                        .HasColumnType("integer")
                        .HasColumnName("INT_PROP_1");

                    b.Property<int?>("IntegerProperty2")
                        .HasColumnType("integer")
                        .HasColumnName("INT_PROP_2");

                    b.Property<long?>("LongProperty1")
                        .HasColumnType("bigint")
                        .HasColumnName("LONG_PROP_1");

                    b.Property<long?>("LongProperty2")
                        .HasColumnType("bigint")
                        .HasColumnName("LONG_PROP_2");

                    b.Property<string>("StringProperty1")
                        .HasColumnType("text")
                        .HasColumnName("STR_PROP_1");

                    b.Property<string>("StringProperty2")
                        .HasColumnType("text")
                        .HasColumnName("STR_PROP_2");

                    b.Property<string>("StringProperty3")
                        .HasColumnType("text")
                        .HasColumnName("STR_PROP_3");

                    b.Property<string>("TimeZoneId")
                        .HasColumnType("text")
                        .HasColumnName("TIME_ZONE_ID");

                    b.HasKey("SchedulerName", "TriggerName", "TriggerGroup");

                    b.ToTable("QRTZ_SIMPROP_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzSimpleTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerName")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<long>("RepeatCount")
                        .HasColumnType("bigint")
                        .HasColumnName("REPEAT_COUNT");

                    b.Property<long>("RepeatInterval")
                        .HasColumnType("bigint")
                        .HasColumnName("REPEAT_INTERVAL");

                    b.Property<long>("TimesTriggered")
                        .HasColumnType("bigint")
                        .HasColumnName("TIMES_TRIGGERED");

                    b.HasKey("SchedulerName", "TriggerName", "TriggerGroup");

                    b.ToTable("QRTZ_SIMPLE_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", b =>
                {
                    b.Property<string>("SchedulerName")
                        .HasColumnType("text")
                        .HasColumnName("SCHED_NAME");

                    b.Property<string>("TriggerName")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_NAME");

                    b.Property<string>("TriggerGroup")
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_GROUP");

                    b.Property<string>("CalendarName")
                        .HasColumnType("text")
                        .HasColumnName("CALENDAR_NAME");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("DESCRIPTION");

                    b.Property<long?>("EndTime")
                        .HasColumnType("bigint")
                        .HasColumnName("END_TIME");

                    b.Property<byte[]>("JobData")
                        .HasColumnType("bytea")
                        .HasColumnName("JOB_DATA");

                    b.Property<string>("JobGroup")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("JOB_GROUP");

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("JOB_NAME");

                    b.Property<short?>("MisfireInstruction")
                        .HasColumnType("smallint")
                        .HasColumnName("MISFIRE_INSTR");

                    b.Property<long?>("NextFireTime")
                        .HasColumnType("bigint")
                        .HasColumnName("NEXT_FIRE_TIME");

                    b.Property<long?>("PreviousFireTime")
                        .HasColumnType("bigint")
                        .HasColumnName("PREV_FIRE_TIME");

                    b.Property<int?>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("PRIORITY");

                    b.Property<long>("StartTime")
                        .HasColumnType("bigint")
                        .HasColumnName("START_TIME");

                    b.Property<string>("TriggerState")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_STATE");

                    b.Property<string>("TriggerType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("TRIGGER_TYPE");

                    b.HasKey("SchedulerName", "TriggerName", "TriggerGroup");

                    b.HasIndex("NextFireTime")
                        .HasDatabaseName("IDX_QRTZ_T_NEXT_FIRE_TIME");

                    b.HasIndex("TriggerState")
                        .HasDatabaseName("IDX_QRTZ_T_STATE");

                    b.HasIndex("NextFireTime", "TriggerState")
                        .HasDatabaseName("IDX_QRTZ_T_NFT_ST");

                    b.HasIndex("SchedulerName", "JobName", "JobGroup");

                    b.ToTable("QRTZ_TRIGGERS", (string)null);
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.Activity", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AreRolesEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatorDisplayName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<ulong>("CreatorUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DueDateTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MaxPlayers")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Name")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ThreadId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId", "ChannelId", "MessageId");

                    b.HasIndex("ThreadId");

                    b.ToTable("Activities");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Activity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.ActivityPlayer", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId", "ChannelId", "MessageId", "UserId");

                    b.ToTable("ActivityPlayers");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ActivityPlayer");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.InterstellarActivity", b =>
                {
                    b.HasBaseType("Cocotte.Modules.Activities.Models.Activity");

                    b.Property<int>("Color")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("InterstellarActivity");
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.StagedActivity", b =>
                {
                    b.HasBaseType("Cocotte.Modules.Activities.Models.Activity");

                    b.Property<uint>("Stage")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("StagedActivity");
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.ActivityRolePlayer", b =>
                {
                    b.HasBaseType("Cocotte.Modules.Activities.Models.ActivityPlayer");

                    b.Property<byte>("Roles")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("ActivityRolePlayer");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzBlobTrigger", b =>
                {
                    b.HasOne("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", "Trigger")
                        .WithMany("BlobTriggers")
                        .HasForeignKey("SchedulerName", "TriggerName", "TriggerGroup")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trigger");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzCronTrigger", b =>
                {
                    b.HasOne("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", "Trigger")
                        .WithMany("CronTriggers")
                        .HasForeignKey("SchedulerName", "TriggerName", "TriggerGroup")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trigger");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzSimplePropertyTrigger", b =>
                {
                    b.HasOne("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", "Trigger")
                        .WithMany("SimplePropertyTriggers")
                        .HasForeignKey("SchedulerName", "TriggerName", "TriggerGroup")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trigger");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzSimpleTrigger", b =>
                {
                    b.HasOne("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", "Trigger")
                        .WithMany("SimpleTriggers")
                        .HasForeignKey("SchedulerName", "TriggerName", "TriggerGroup")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trigger");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", b =>
                {
                    b.HasOne("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzJobDetail", "JobDetail")
                        .WithMany("Triggers")
                        .HasForeignKey("SchedulerName", "JobName", "JobGroup")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JobDetail");
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.ActivityPlayer", b =>
                {
                    b.HasOne("Cocotte.Modules.Activities.Models.Activity", "Activity")
                        .WithMany("ActivityPlayers")
                        .HasForeignKey("GuildId", "ChannelId", "MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Activity");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzJobDetail", b =>
                {
                    b.Navigation("Triggers");
                });

            modelBuilder.Entity("AppAny.Quartz.EntityFrameworkCore.Migrations.QuartzTrigger", b =>
                {
                    b.Navigation("BlobTriggers");

                    b.Navigation("CronTriggers");

                    b.Navigation("SimplePropertyTriggers");

                    b.Navigation("SimpleTriggers");
                });

            modelBuilder.Entity("Cocotte.Modules.Activities.Models.Activity", b =>
                {
                    b.Navigation("ActivityPlayers");
                });
#pragma warning restore 612, 618
        }
    }
}
