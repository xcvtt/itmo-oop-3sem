﻿using Mps.Domain.ValueObjects;

namespace Mps.Domain.Device;

public class PhoneDevice : DeviceBase
{
    public PhoneDevice(Guid id, PhoneNumber phoneNumber)
        : base(id)
    {
        PhoneNumber = phoneNumber;
    }

    private PhoneDevice()
    {
        PhoneNumber = null!;
    }

    public PhoneNumber PhoneNumber { get; private set; }
}