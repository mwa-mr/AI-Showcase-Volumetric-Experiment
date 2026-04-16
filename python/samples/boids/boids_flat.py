'''
This is a simple boids simulation and uses Pygame to render the boids in a 2D Window.
'''
import random
import math
import cProfile
from pstats import Stats
import pygame

# Initialize Pygame
pygame.init()

# Screen dimensions
WIDTH, HEIGHT = 750, 750
DEPTH = 750  # Depth (z-axis) for 3D space
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("3D Boids with Target Chasing")

volume_size = pygame.Vector3(1, 1, 1)


class Boid:
    '''
    Boid class represents a single boid in the simulation.
    '''
    position = pygame.Vector3()
    velocity = pygame.Vector3()
    rotation = pygame.Vector3()
    orientation = [0, 0, 0, 0]
    color = (1, 1, 1)
    seed = random.uniform(-1, 1)
    neighbors = []

    def __init__(self):
        self.position = .5 * pygame.Vector3(random.uniform(-volume_size.x/2, volume_size.x/2),
                                            random.uniform(-volume_size.y / 2, volume_size.y/2),
                                            random.uniform(-volume_size.z/2, volume_size.z/2))
        self.velocity = .1 * pygame.Vector3(random.uniform(-1, 1), random.uniform(-1, 1), random.uniform(-1, 1)).normalize()
        self.color = (random.randint(0, 255), random.randint(0, 255), random.randint(0, 255))

    def update(self, time_elapsed, _boids, target):
        '''
        Update the boid's position and velocity based on the rules of the simulation.
        '''

        def get_distance(self, other):
            distance = self.position.distance_to(other.position)
            return distance

        # Periodically find a new set of neighbors - allowing subgroups to form
        if (len(self.neighbors) == 0 or random.random() < .05):
            distances = [get_distance(self, other) for other in _boids]
            sorted_pairs = sorted(zip(distances, _boids), reverse=False)
            self.neighbors = [pair[1] for pair in sorted_pairs][:15]

        # Cohesion: Move towards the center of mass of nearby boids
        center_of_mass = pygame.Vector3(0, 0, 0)
        num_neighbors = 0
        for other in self.neighbors:
            if other != self and get_distance(self, other) < .3:
                center_of_mass += other.position
                num_neighbors += 1

        if num_neighbors > 0:
            center_of_mass /= num_neighbors
            self.velocity += (center_of_mass - self.position) * 1.0

        # Avoidance: Avoid collisions with nearby boids
        for other in self.neighbors:
            if other != self:
                distance = self.position.distance_to(other.position)
                if distance < .035:
                    self.velocity -= (other.position - self.position) * 0.15

        desired_velocity = pygame.Vector3(0, 0, 0)
        # Target chasing: Move towards the target (mouse) or center if nearing edge of the volume
        if self.position.distance_to(pygame.Vector3(0, 0, 0)) < volume_size.x/2:
            desired_velocity = (target - self.position).normalize() * (0.025 + self.seed * .015)
        else:
            desired_velocity = (pygame.Vector3(0, 0, 0) - self.position).normalize() * 0.1

        self.velocity += desired_velocity

        # Randomness: Add some noise to velocity
        self.velocity += .02 * pygame.Vector3(random.uniform(-1, 1), random.uniform(-1, 1), random.uniform(-1, 1)).normalize()

        # Limit speed
        self.velocity.scale_to_length(1.5)

        # Update position
        self.position += self.velocity * .2 * time_elapsed

        # Update orientation
        direction = (self.velocity).normalize()
        fwd = pygame.Vector3(0, 0, -1)
        a = direction.cross(fwd)
        self.orientation = [a.x, a.y, a.z, math.sqrt(pow(direction.length(), 2) * pow(fwd.length(), 2)) + direction.dot(fwd)]

    def draw(self):
        '''
        Draw the boid on the screen.
        '''
        # Project 3D position to 2D screen coordinates
        x, y = (self.position.x + volume_size.x/2) * WIDTH, (self.position.y + volume_size.y/2) * HEIGHT
        z = self.position.z
        screen_x = int(x * (WIDTH / DEPTH))
        screen_y = int(y * (HEIGHT / DEPTH))
        pygame.draw.circle(screen, self.color, (screen_x, screen_y), 3 + int(z / DEPTH * (WIDTH / volume_size.x)*8))


BLACK = (0, 0, 0)
RED = (255, 0, 0)


class FPS:
    '''
    Class to display frames per second on the screen.
    '''
    last_fps = 0

    def __init__(self):
        self.font: pygame.font.Font = pygame.font.SysFont("Verdana", 12)
        self.text: str = None

    def render(self, display, fps, color=RED):
        '''
        Render the frames per second on the screen.
        '''
        self.text = self.font.render(str(int(round(fps, 0))) + " fps", True, color)
        display.blit(self.text, (10, 10))


class Boids:
    '''
    Boids class represents the simulation of multiple boids.
    '''
    running = True
    num_boids = 0
    framerate = 0
    boids = None
    clock = pygame.time.Clock()

    fps = FPS()

    target_position = pygame.Vector3(0, 0, 0)
    last_tick = pygame.time.get_ticks()

    def __init__(self, num_boids=75, framerate=36):
        self.num_boids = num_boids
        self.framerate = framerate
        self.boids = [Boid() for _ in range(num_boids)]

    # Main loop when run standalone.
    def run(self):
        '''
        Main loop to run the simulation.
        '''
        while self.running:
            self.clock.tick(self.framerate)
            self.update()

        pygame.quit()

    lastfps = 0

    def update(self):
        '''
        Update the simulation of boids by computing their new positions and velocities.
        '''
        if not self.running:
            return

        ticks = pygame.time.get_ticks()
        ticks_elapsed = ticks - self.last_tick

        # Time elapsed in seconds
        # Limit to 0.1 seconds to avoid large time steps
        time_elapsed = min(.1, ticks_elapsed / 1000)
        self.last_tick = ticks

        screen.fill((255, 255, 255))

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                print("Quit")
                self.running = False
            elif event.type == pygame.WINDOWCLOSE:
                print("Window closed")
                self.running = False

        mouse_pos = pygame.Vector2(0, 0)
        mouse_pos.x, mouse_pos.y = pygame.mouse.get_pos()
        self.target_position.x = (mouse_pos.x - WIDTH / 2) / WIDTH * volume_size.x
        self.target_position.y = (mouse_pos.y - HEIGHT / 2) / HEIGHT * volume_size.y

        # print(self.target_position)

        for boid in self.boids:
            boid.update(time_elapsed, self.boids, self.target_position)
            boid.draw()

        fps = 1 / time_elapsed
        # smooth over time
        self.lastfps = (self.lastfps * 49 + fps) / 50
        self.fps.render(screen, self.lastfps, RED if self.lastfps < self.framerate else BLACK)
        pygame.display.flip()


if __name__ == '__main__':
    print("====")
    print("boids_flat.py shows 2D boids on Desktop.")
    print("To run 3D boids in Volumetric, run boids_volumetric.py instead.")
    print("====")

    boids = Boids(num_boids=75, framerate=72)

    do_profiling: bool = False
    if do_profiling:
        with cProfile.Profile() as pr:
            boids.run()

        with open('profiling_stats.txt', 'w', encoding="utf-8") as stream:
            stats = Stats(pr, stream=stream)
            stats.strip_dirs()
            stats.sort_stats('time')
            stats.dump_stats('.prof_stats')
            stats.print_stats()
    else:
        boids.run()
