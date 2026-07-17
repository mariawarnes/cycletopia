using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace CycletopiaMod
{
    public sealed class Cycletopia : Script
    {
        private readonly Random random = new Random();
        private readonly HashSet<int> handled = new HashSet<int>();
        private readonly VehicleHash[] bicycles =
        {
            VehicleHash.Bmx,
            VehicleHash.Cruiser,
            VehicleHash.Fixter,
            VehicleHash.Scorcher,
            VehicleHash.TriBike,
            VehicleHash.TriBike2,
            VehicleHash.TriBike3
        };
        private readonly PedHash[] riders =
        {
            PedHash.Cyclist01,
            PedHash.Cyclist01AMY,
            PedHash.Hiker01AFY
        };

        private bool enabled;
        private float radius;
        private float cruiseSpeed;
        private int replacementsPerTick;
        private int nextCleanup;

        public Cycletopia()
        {
            Interval = 400;
            LoadSettings();
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;
            GTA.UI.Notification.Show("~g~Cycletopia~s~ loaded. Press ~y~F7~s~ to toggle.");
        }

        private void LoadSettings()
        {
            ScriptSettings settings = ScriptSettings.Load(@"scripts\Cycletopia.ini");
            enabled = settings.GetValue("Cycletopia", "Enabled", true);
            radius = Clamp(settings.GetValue("Cycletopia", "ReplacementRadius", 120.0f), 30.0f, 300.0f);
            cruiseSpeed = Clamp(settings.GetValue("Cycletopia", "CyclistSpeed", 9.0f), 3.0f, 18.0f);
            replacementsPerTick = Math.Max(1, Math.Min(12, settings.GetValue("Cycletopia", "ReplacementsPerTick", 4)));
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F7)
                return;

            enabled = !enabled;
            GTA.UI.Notification.Show(enabled
                ? "~g~Cycletopia enabled"
                : "~r~Cycletopia disabled");
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!enabled || Game.Player == null || Game.Player.Character == null)
                return;

            Ped player = Game.Player.Character;
            Vector3 playerPosition = player.Position;
            Vehicle playerVehicle = player.CurrentVehicle;
            int replaced = 0;

            foreach (Vehicle vehicle in World.GetAllVehicles())
            {
                if (replaced >= replacementsPerTick)
                    break;
                if (!IsAmbientCar(vehicle, playerVehicle, playerPosition))
                    continue;

                handled.Add(vehicle.Handle);
                if (ReplaceWithCyclist(vehicle))
                    replaced++;
            }

            if (Game.GameTime >= nextCleanup)
            {
                handled.Clear();
                nextCleanup = Game.GameTime + 15000;
            }
        }

        private bool IsAmbientCar(Vehicle vehicle, Vehicle playerVehicle, Vector3 playerPosition)
        {
            if (vehicle == null || !vehicle.Exists() || vehicle.IsDead)
                return false;
            if (playerVehicle != null && vehicle.Handle == playerVehicle.Handle)
                return false;
            if (handled.Contains(vehicle.Handle) || vehicle.IsPersistent)
                return false;
            if (vehicle.Position.DistanceToSquared(playerPosition) > radius * radius)
                return false;

            int vehicleClass = Function.Call<int>(Hash.GET_VEHICLE_CLASS, vehicle.Handle);
            if (vehicleClass == 13 || vehicleClass == 14 || vehicleClass == 15 ||
                vehicleClass == 16 || vehicleClass == 21)
                return false;

            Ped driver = vehicle.Driver;
            if (driver == null || !driver.Exists() || driver.IsPlayer || driver.IsPersistent)
                return false;

            return !Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, vehicle.Handle);
        }

        private bool ReplaceWithCyclist(Vehicle car)
        {
            Vector3 position = car.Position;
            float heading = car.Heading;
            float speed = Math.Max(4.0f, Math.Min(cruiseSpeed, car.Speed));
            Ped oldDriver = car.Driver;

            Model bicycleModel = new Model(bicycles[random.Next(bicycles.Length)]);
            Model riderModel = new Model(riders[random.Next(riders.Length)]);
            if (!bicycleModel.Request(250) || !riderModel.Request(250))
                return false;

            Vehicle bicycle = World.CreateVehicle(bicycleModel, position, heading);
            if (bicycle == null || !bicycle.Exists())
                return false;

            Ped rider = World.CreatePed(riderModel, position);
            if (rider == null || !rider.Exists())
            {
                bicycle.Delete();
                return false;
            }

            if (oldDriver != null && oldDriver.Exists())
                oldDriver.Delete();
            car.Delete();

            rider.SetIntoVehicle(bicycle, VehicleSeat.Driver);
            Function.Call(Hash.SET_ENTITY_VELOCITY, bicycle.Handle,
                bicycle.ForwardVector.X * speed, bicycle.ForwardVector.Y * speed, 0.0f);
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, rider.Handle, true);
            Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, rider.Handle, bicycle.Handle,
                cruiseSpeed, 786603);
            rider.MarkAsNoLongerNeeded();
            bicycle.MarkAsNoLongerNeeded();
            bicycleModel.MarkAsNoLongerNeeded();
            riderModel.MarkAsNoLongerNeeded();
            return true;
        }

        private void OnAborted(object sender, EventArgs e)
        {
            handled.Clear();
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
