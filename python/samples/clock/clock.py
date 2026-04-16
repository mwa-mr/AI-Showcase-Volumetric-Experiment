'''
This sample demonstrates a volumetric clock using Volumetric Python SDK.
    The clock hands rotate according to the current time.
    The clock also displays the current time and timezone in AdaptiveCard.
'''
import math

import datetime
import dataclasses
import os
from zoneinfo import available_timezones, ZoneInfo  # pip import tzdata
from tzlocal import get_localzone  # pip import tzlocal
import volumetric as va


class _Clock(va.Volume):

    @dataclasses.dataclass
    class _Elements:
        adaptive_card: va.AdaptiveCardElement = None
        model: va.ModelResource = None
        visual: va.VisualElement = None
        hour_hand: va.VisualElement = None
        minute_hand: va.VisualElement = None
        second_hand: va.VisualElement = None

    def __init__(self, _app: va.VolumetricApp):
        super().__init__(_app)
        script_dir = os.path.dirname(os.path.abspath(__file__))
        self._uri = _app.get_local_asset_uri(os.path.join(script_dir, "../../../assets/clock.glb"))
        self.container.set_display_name("Volumetric Clock")
        self.container.set_rotation_lock(va.VA_VOLUME_ROTATION_LOCK_X | va.VA_VOLUME_ROTATION_LOCK_Z)
        self._elements = _Clock._Elements()
        self._adaptive_card_template: str = """
            {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "id": "time",
                        "type": "TextBlock",
                        "text": "${current_time}",
                        "horizontalAlignment": "center"
                    },
                    {
                        "id": "timezone",
                        "type": "TextBlock",
                        "text": "${timezone}",
                        "horizontalAlignment": "center"
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Execute",
                        "title": "Timezone -",
                        "verb": "dec"
                    },
                    {
                        "type": "Action.Execute",
                        "title": "Timezone +",
                        "verb": "inc"
                    }
                ],
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.4"
            }
            """
        self.timezone_index: int = -1
        self.timezones = list(available_timezones())
        if len(self.timezones) == 0:
            raise RuntimeError("Missing timezone data, use pip install tzdata")
        local_timezone = get_localzone()
        for i, timezone in enumerate(self.timezones):
            if timezone == local_timezone.key:
                self.timezone_index = i
                break
        self.container.set_thumbnail_icon_uri(_app.get_local_asset_uri("../../../assets/ClockThumbnail.png"))
        self.container.set_thumbnail_model_uri(_app.get_local_asset_uri("../../../assets/ClockThumbnail.glb"))
        print(f"Local timezone: {local_timezone.key}, index={self.timezone_index}")

    def _format_adaptive_card_data(self, now: datetime.datetime) -> str:
        current_time = now.strftime("%a %m/%d/%y %H:%M:%S")
        timezone_name = self.timezones[self.timezone_index]
        offset = now.utcoffset()
        offset_hours = int(offset.total_seconds() / 3600)
        offset_minutes = int(abs(offset.seconds) % 3600 / 60)
        sign = "+" if offset_hours >= 0 else "-"
        return f'{{"current_time":"{current_time}","timezone":"(UTC{sign}{abs(offset_hours):02d}:{offset_minutes:02d}) {timezone_name}"}}'

    def on_ready(self) -> None:
        tz = ZoneInfo(self.timezones[self.timezone_index])
        now = datetime.datetime.now(tz)

        # Setup the digital clock using Adaptive Card
        self._elements.adaptive_card = va.AdaptiveCardElement(self, self._on_adaptive_card_action)
        self._elements.adaptive_card.set_template(self._adaptive_card_template)
        self._elements.adaptive_card.set_data(self._format_adaptive_card_data(now))

        # Load the 3D clock model
        self._elements.model = va.ModelResource(self, uri=self._uri)
        self._elements.visual = va.VisualElement.create_with_visual_resource(self, visual_resource=self._elements.model)

        # Create visual elements for the clock hands
        self._elements.hour_hand = va.VisualElement.create_named_node(self, visual_reference=self._elements.visual, node_name="def_hours")
        self._elements.minute_hand = va.VisualElement.create_named_node(self, visual_reference=self._elements.visual, node_name="def_minutes")
        self._elements.second_hand = va.VisualElement.create_named_node(self, visual_reference=self._elements.visual, node_name="def_seconds")

        # This clock sample requests update every 1 second
        self.request_update_after_seconds(1)

    def on_update(self) -> None:
        # print("_Clock.volumeOnUpdate")
        # This clock sample requests update every 1 second
        self.request_update_after_seconds(1)

        tz = ZoneInfo(self.timezones[self.timezone_index])
        now = datetime.datetime.now(tz)
        hours = now.hour
        minutes = now.minute
        seconds = now.second
        # print(f"Set time {hours}:{minutes}:{seconds} timezone_index={self.timezone_index}")

        # calculate the angles for the clock hands
        hour_angle = ((hours * 60 + minutes) * 0.5) % 360
        minute_angle = ((minutes * 60 + seconds) * 0.1) % 360
        second_angle = (seconds * 1000) * 0.006

        def quaternion_from_eular(pitch, yaw, roll):
            cp = math.cos(pitch * 0.5)
            sp = math.sin(pitch * 0.5)
            cy = math.cos(yaw * 0.5)
            sy = math.sin(yaw * 0.5)
            cr = math.cos(roll * 0.5)
            sr = math.sin(roll * 0.5)

            x = cr * sp * cy + sr * cp * sy
            y = cr * cp * sy - sr * sp * cy
            z = sr * cp * cy - cr * sp * sy
            w = cr * cp * cy + sr * sp * sy

            return [x, y, z, w]

        # The rotation must be consistent with the corresponding gltf node local transform in Glb file.
        # In this case, the clock.glb file contains "def_hours", "def_minutes" and "def_seconds" nodes,
        # and they are rotated 90 degrees around the X axis, and clockwise rotation is around the Y axis.
        # One can inspect the gltf node transforms in a gltf viewer, such as https://sandbox.babylonjs.com/
        hour_quaternion = quaternion_from_eular(math.radians(90), math.radians(hour_angle), 0)
        minute_quaternion = quaternion_from_eular(math.radians(90), math.radians(minute_angle), 0)
        second_quaternion = quaternion_from_eular(math.radians(90), math.radians(second_angle), 0)

        # set the rotation of the clock hands in quaternions
        self._elements.hour_hand.set_orientation(hour_quaternion[0], hour_quaternion[1], hour_quaternion[2], hour_quaternion[3])
        self._elements.minute_hand.set_orientation(minute_quaternion[0], minute_quaternion[1], minute_quaternion[2], minute_quaternion[3])
        self._elements.second_hand.set_orientation(second_quaternion[0], second_quaternion[1], second_quaternion[2], second_quaternion[3])

        # update Adaptive Card data
        self._elements.adaptive_card.set_data(self._format_adaptive_card_data(now))

    def _on_adaptive_card_action(self, verb: str):
        print(f"Clock._on_adaptive_card_action verb={verb}")
        if verb == "inc":
            self.timezone_index = (self.timezone_index + 1) % len(self.timezones)
        elif verb == "dec":
            self.timezone_index = (self.timezone_index - 1) % len(self.timezones)

    def on_close(self) -> None:
        self._elements = None
        # This sample has only one volume. Hence, the app exits when the user closes the volume.
        self.app.request_exit()


if __name__ == '__main__':
    app = va.VolumetricApp("Python Clock", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                            va.VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME,
                                            va.VA_EXT_VOLUME_CONTAINER_THUMBNAIL_EXTENSION_NAME])
    app.on_start = lambda _: _Clock(app)
    app.run()
    print("Done.")
