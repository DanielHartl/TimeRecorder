namespace ActivityTracker.Client.App
{
    internal class MouseEvent
    {
        public MouseEvent(MouseEventType mouseEventType, MousePoint mousePoint)
        {
            MouseEventType = mouseEventType;
            MousePoint = mousePoint;
        }

        public MouseEventType MouseEventType { get; }
        public MousePoint MousePoint { get; }
    }
}